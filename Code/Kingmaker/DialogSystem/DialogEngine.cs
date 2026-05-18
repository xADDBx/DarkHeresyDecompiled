using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.DialogSystem.State;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.DialogSystem;

public class DialogEngine
{
	public delegate void DialogEventHandler(IDialogContext dialog);

	public delegate void CueEventHandler(BlueprintCue cue);

	public delegate void ShowCueEventHandler(CueShowData cueShowData);

	public delegate void BookPageEventHandler(BlueprintBookPage cue);

	public delegate void ShownBookPageEventHandler(BlueprintBookPage page, List<CueShowData> bookPageCues, List<BlueprintAnswer> answers);

	public delegate void AnswerEventHandler(BlueprintAnswer answer);

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("Dialog");

	[NotNull]
	public readonly HashSet<BlueprintCueBase> LocalShownCues = new HashSet<BlueprintCueBase>();

	[NotNull]
	public readonly HashSet<BlueprintAnswer> LocalSelectedAnswers = new HashSet<BlueprintAnswer>();

	[NotNull]
	public readonly HashSet<BlueprintAnswersList> LocalShownAnswerLists = new HashSet<BlueprintAnswersList>();

	[NotNull]
	public readonly HashSet<BlueprintCheck> LocalPassedChecks = new HashSet<BlueprintCheck>();

	[NotNull]
	public readonly HashSet<BlueprintCheck> LocalFailedChecks = new HashSet<BlueprintCheck>();

	public Func<BaseUnitEntity, BlueprintCheck, SkillCheckResult> OverrideSkillCheckResult;

	[CanBeNull]
	private DialogState m_DialogState;

	private IDialogContext m_Context;

	private CueShowData m_CurrentCueShowData;

	private bool m_CapitalPartyChecksEnabled;

	private readonly List<BlueprintAnswer> m_Answers = new List<BlueprintAnswer>();

	[CanBeNull]
	private BlueprintCueBase m_ContinueCue;

	private BlueprintCueBase m_CueToPlay;

	private int m_CuesPlayedThisFrame;

	private int m_PlayingBookPageCount;

	private readonly List<CueShowData> m_BookPageCues = new List<CueShowData>();

	[NotNull]
	private readonly Stack<CueSequence> m_Sequences = new Stack<CueSequence>();

	[NotNull]
	private readonly List<SkillCheckResult> m_SkillChecks = new List<SkillCheckResult>();

	public BlueprintCue CurrentCue { get; private set; }

	public bool CuePlayScheduled { get; private set; }

	public CueShowData CurrentCueShowData => m_CurrentCueShowData;

	public BlueprintDialog Dialog => m_Context?.Dialog;

	public string CurrentSpeakerName => m_Context?.CurrentSpeakerName;

	public BlueprintUnit CurrentSpeakerBlueprint => m_Context?.CurrentSpeakerBlueprint;

	public bool IsPlayingBookPage => m_PlayingBookPageCount > 0;

	public IEnumerable<BlueprintAnswer> Answers => m_Answers;

	public event DialogEventHandler StoppingDialog;

	public event CueEventHandler ExitedCue;

	public event CueEventHandler EnteringCue;

	public event ShowCueEventHandler ShowingCue;

	public event BookPageEventHandler ShowingBookPage;

	public event ShownBookPageEventHandler ShownBookPage;

	public event AnswerEventHandler SelectingAnswer;

	public event AnswerEventHandler SelectedAnswer;

	public void StartDialog([NotNull] IDialogContext dialogContext, [NotNull] DialogState dialogState)
	{
		m_Context = dialogContext;
		m_DialogState = dialogState;
		Logger.Log(Dialog, "Trying to start dialog {0}", Dialog);
		ScheduleCue(Dialog.FirstCue.Select());
		DialogDebug.Add(Dialog, "Started dialog", Color.green);
		m_DialogState.ShownDialogsAdd(Dialog);
	}

	public void TryPlayNextCue()
	{
		if (CuePlayScheduled)
		{
			m_CuesPlayedThisFrame = 0;
			CuePlayScheduled = false;
			PlayCue(m_CueToPlay);
		}
	}

	public void SelectAnswer(string answerGuid)
	{
		int i = 0;
		for (int count = m_Answers.Count; i < count; i++)
		{
			BlueprintAnswer blueprintAnswer = m_Answers[i];
			if (blueprintAnswer.AssetGuid == answerGuid)
			{
				SelectAnswer(blueprintAnswer);
				break;
			}
		}
	}

	public void SelectAnswer(BlueprintAnswer answer, BaseUnitEntity manualUnitSelection = null)
	{
		if (CurrentCue == null)
		{
			return;
		}
		if (!answer.IsSystem() && !m_Answers.Contains(answer))
		{
			PFLog.Default.Error("Trying to select invalid dialog answer {0}", answer);
			return;
		}
		if (manualUnitSelection == null && answer.CharacterSelection.SelectionType == CharacterSelection.Type.Manual)
		{
			PFLog.Default.Error("A unit must be specified for selected answer. Answer: {0}", answer);
			return;
		}
		this.SelectingAnswer?.Invoke(answer);
		DialogDebug.Add(answer, "chosen answer");
		LocalSelectedAnswers.Add(answer);
		if (m_DialogState != null)
		{
			m_DialogState.SelectedAnswersAdd(answer);
			Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> bookEventLog = m_DialogState.BookEventLog;
			if (bookEventLog.ContainsKey(Dialog))
			{
				bookEventLog[Dialog].Add(answer);
			}
		}
		m_Context.Update(answer, manualUnitSelection);
		if (answer.CharacterSelection.SelectionType != CharacterSelection.Type.Keep)
		{
			m_CapitalPartyChecksEnabled = answer.CapitalPartyChecksEnabled;
		}
		answer.OnSelect.Run();
		BlueprintCueBase cue = SelectNextCue(answer);
		ScheduleCue(cue);
		this.SelectedAnswer?.Invoke(answer);
	}

	public void FinishDialog()
	{
		Dialog?.FinishActions.Run();
		Clear();
	}

	public void Clear()
	{
		CurrentCue?.OnStop.Run();
		this.ExitedCue?.Invoke(CurrentCue);
		CurrentCue = null;
		m_CapitalPartyChecksEnabled = false;
		m_CueToPlay = null;
		CuePlayScheduled = false;
		m_Answers.Clear();
		m_ContinueCue = null;
		m_Sequences.Clear();
		m_BookPageCues.Clear();
		m_SkillChecks.Clear();
		m_PlayingBookPageCount = 0;
		LocalShownCues.Clear();
		LocalSelectedAnswers.Clear();
		LocalShownAnswerLists.Clear();
		LocalPassedChecks.Clear();
		LocalFailedChecks.Clear();
		OverrideSkillCheckResult = null;
		m_Context = null;
		m_DialogState = null;
	}

	private void ScheduleCue(BlueprintCueBase cue)
	{
		m_CueToPlay = cue;
		CuePlayScheduled = true;
	}

	private void StopDialog()
	{
		this.StoppingDialog?.Invoke(m_Context);
		FinishDialog();
	}

	[CanBeNull]
	private BlueprintCueBase SelectNextCue([NotNull] BlueprintAnswer answer)
	{
		if (answer.IsContinue())
		{
			if (m_ContinueCue == null)
			{
				PFLog.Default.Error("Continue answer was selected but continue cue is not specified");
			}
			return m_ContinueCue;
		}
		BlueprintCueBase blueprintCueBase = (answer.IsExit() ? null : answer.NextCue.Select());
		while (blueprintCueBase == null && m_Sequences.Any())
		{
			CueSequence cueSequence = m_Sequences.Peek();
			blueprintCueBase = cueSequence.PollNextCue();
			if (blueprintCueBase == null)
			{
				m_Sequences.Pop();
				blueprintCueBase = cueSequence.Blueprint.Exit?.Continue.Select();
			}
		}
		return blueprintCueBase;
	}

	private void PlayCue(BlueprintCueBase cue)
	{
		if (m_CuesPlayedThisFrame++ > 1000)
		{
			throw new InvalidOperationException($"Stack overflow while playing dialog cues. Dialog: {Dialog}, One of cues: {cue}");
		}
		if (cue == null)
		{
			StopDialog();
			return;
		}
		LocalShownCues.Add(cue);
		m_DialogState?.ShownCuesAdd(cue);
		m_Answers.Clear();
		m_ContinueCue = null;
		DialogDebug.Add(cue, "played", Color.green);
		if (cue is BlueprintCue cue2)
		{
			PlayBasicCue(cue2);
		}
		else if (cue is BlueprintCheck check)
		{
			PlayCheck(check);
		}
		else if (cue is BlueprintCueSequence sequence)
		{
			PlaySequence(sequence);
		}
		else if (cue is BlueprintBookPage page)
		{
			PlayBookPage(page);
		}
	}

	private void AddAnswers([NotNull] IEnumerable<BlueprintAnswerBase> answers, [CanBeNull] BlueprintCueBase continueCue)
	{
		BlueprintAnswerBase[] array = (answers as BlueprintAnswerBase[]) ?? answers.ToArray();
		if (continueCue == null)
		{
			BlueprintAnswerBase[] array2 = array;
			for (int j = 0; j < array2.Length; j++)
			{
				if (array2[j] is BlueprintAnswersList blueprintAnswersList && blueprintAnswersList.CanSelect())
				{
					m_DialogState?.ShownAnswerListsAdd(blueprintAnswersList);
					LocalShownAnswerLists.Add(blueprintAnswersList);
					AddAnswers(blueprintAnswersList.Answers.Dereference(), null);
					EventBus.RaiseEvent(delegate(IDialogAnswersShownHandler i)
					{
						i.HandleAnswersShown();
					});
					return;
				}
			}
		}
		m_Answers.Clear();
		m_ContinueCue = null;
		if ((bool)continueCue)
		{
			m_Answers.Add(Dialog.GetContinueAnswer());
			m_ContinueCue = continueCue;
		}
		else
		{
			BlueprintAnswerBase[] array2 = array;
			foreach (BlueprintAnswerBase blueprintAnswerBase in array2)
			{
				BlueprintAnswer answer = blueprintAnswerBase as BlueprintAnswer;
				if (answer != null && answer.CanShow())
				{
					m_Answers.Add(answer);
					EventBus.RaiseEvent(delegate(IDialogAnswersAddedToPoolHandler h)
					{
						h.HandleDialogAnswersAddedToPool(answer);
					});
				}
			}
		}
		if (Answers.Any())
		{
			return;
		}
		if (m_Sequences.Count > 0)
		{
			CueSequence cueSequence = m_Sequences.Peek();
			BlueprintCueBase blueprintCueBase = cueSequence.PollNextCue();
			if (blueprintCueBase != null)
			{
				m_Answers.Add(Dialog.GetContinueAnswer());
				m_ContinueCue = blueprintCueBase;
				return;
			}
			m_Sequences.Pop();
			BlueprintSequenceExit exit = cueSequence.Blueprint.Exit;
			if ((bool)exit)
			{
				AddAnswers(exit.Answers.Dereference(), exit.Continue.Select());
			}
			else
			{
				m_Answers.Add(Dialog.GetExitAnswer());
			}
		}
		else
		{
			m_Answers.Add(Dialog.GetExitAnswer());
		}
	}

	private void PlayBasicCue(BlueprintCue cue)
	{
		CurrentCue?.OnStop.Run();
		this.ExitedCue?.Invoke(CurrentCue);
		CurrentCue = cue;
		m_Context.Update(CurrentCue);
		this.EnteringCue?.Invoke(CurrentCue);
		CurrentCue?.OnShow.Run();
		BlueprintCueBase blueprintCueBase = cue.Continue.Select();
		CueShowData cueShowData = new CueShowData(cue, m_SkillChecks);
		m_SkillChecks.Clear();
		if (!IsPlayingBookPage)
		{
			AddAnswers(cue.Answers.Dereference(), blueprintCueBase);
			m_CurrentCueShowData = cueShowData;
		}
		else
		{
			m_BookPageCues.Add(cueShowData);
			m_DialogState?.BookEventLog[Dialog].Add(cue);
		}
		this.ShowingCue?.Invoke(cueShowData);
		if (IsPlayingBookPage && (bool)blueprintCueBase)
		{
			PlayCue(blueprintCueBase);
		}
	}

	private SkillCheckResult ExecuteCheck(BlueprintCheck check)
	{
		BaseUnitEntity baseUnitEntity = check.GetTargetUnit() ?? m_Context?.ActingUnit;
		SkillCheckResult skillCheckResult = OverrideSkillCheckResult?.Invoke(baseUnitEntity, check);
		if (skillCheckResult != null)
		{
			return skillCheckResult;
		}
		try
		{
			if (baseUnitEntity != null)
			{
				return new SkillCheckResult(GameHelper.TriggerSkillCheck(new RulePerformSkillCheck(baseUnitEntity, check.Type, check.GetDC()), null, allowPartyCheckInCamp: false), baseUnitEntity);
			}
			RulePerformPartySkillCheck rulePerformPartySkillCheck = new RulePerformPartySkillCheck(check.Type, check.GetDC(), m_CapitalPartyChecksEnabled);
			Rulebook.Instance.TriggerEvent(rulePerformPartySkillCheck);
			return new SkillCheckResult(rulePerformPartySkillCheck);
		}
		catch (Exception ex)
		{
			Logger.Exception(ex, "Failed to perform skill check {0}", check);
			return new SkillCheckResult(baseUnitEntity ?? Game.Instance.Player.MainCharacterEntity, check.Type, check.GetDC());
		}
	}

	private void PlayCheck(BlueprintCheck check)
	{
		SkillCheckResult skillCheckResult = ExecuteCheck(check);
		if (skillCheckResult.Passed)
		{
			LocalPassedChecks.Add(check);
			LocalFailedChecks.Remove(check);
			Experience.GainForSkillCheck(check.Difficulty, skillCheckResult.DC, skillCheckResult.ActingUnit);
			check.OnCheckSuccessActions?.Run();
		}
		else if (!LocalPassedChecks.Contains(check))
		{
			LocalFailedChecks.Add(check);
			check.OnCheckFailActions?.Run();
		}
		if (!check.Hidden || skillCheckResult.Passed)
		{
			m_SkillChecks.Add(skillCheckResult);
		}
		PlayCue(skillCheckResult.Passed ? check.Success : check.Fail);
	}

	private void PlaySequence(BlueprintCueSequence sequence)
	{
		CueSequence cueSequence = new CueSequence(sequence);
		m_Sequences.Push(cueSequence);
		BlueprintCueBase blueprintCueBase = cueSequence.PollNextCue();
		if (blueprintCueBase == null)
		{
			PFLog.Default.Error("Could not select first cue in cue sequence ({0}).", sequence);
			StopDialog();
		}
		else
		{
			PlayCue(blueprintCueBase);
		}
	}

	private void PlayBookPage(BlueprintBookPage page)
	{
		try
		{
			m_PlayingBookPageCount++;
			this.ShowingBookPage?.Invoke(page);
			if (m_DialogState != null)
			{
				Dictionary<BlueprintDialog, List<BlueprintScriptableObject>> bookEventLog = m_DialogState.BookEventLog;
				if (!bookEventLog.ContainsKey(Dialog))
				{
					bookEventLog.Add(Dialog, new List<BlueprintScriptableObject>());
				}
				bookEventLog[Dialog].Add(page);
			}
			page.OnShow.Run();
			IEnumerable<BlueprintCueBase> enumerable = from cue in page.Cues.Dereference()
				where cue.CanShow()
				select cue;
			bool flag = false;
			foreach (BlueprintCueBase item in enumerable)
			{
				PlayCue(item);
				flag = true;
			}
			if (!flag)
			{
				LogChannel.Default.ErrorWithReport("Could not select any cue in book page ({0}).", page);
				StopDialog();
			}
			else
			{
				AddAnswers(page.Answers.Dereference(), null);
				this.ShownBookPage?.Invoke(page, m_BookPageCues, m_Answers);
			}
		}
		finally
		{
			m_BookPageCues.Clear();
			m_PlayingBookPageCount--;
		}
	}
}
