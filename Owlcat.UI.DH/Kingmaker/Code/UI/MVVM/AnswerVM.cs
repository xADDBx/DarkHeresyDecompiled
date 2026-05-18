using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Designers.EventConditionActionSystem.Conditions;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.GameCommands;
using Kingmaker.Networking;
using Kingmaker.Networking.Player;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Common.DebugInformation;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using UnityEngine;
using WebSocketSharp;

namespace Kingmaker.Code.UI.MVVM;

public class AnswerVM : ViewModel, IBookEventUIHandler, ISubscriber, INetPingDialogAnswer, IHasBlueprintInfo
{
	public readonly BlueprintAnswer Answer;

	public readonly List<SkillCheckDC> SkillChecks;

	public readonly List<SkillCheckDC> SkillCheckRequirements;

	public readonly int Index;

	public readonly string BindingName;

	public readonly bool IsSystem;

	private readonly string m_TextByBinding;

	private readonly DialogController m_DialogController;

	public static readonly ReactiveProperty<string> ChoosedAnswer = new ReactiveProperty<string>();

	public readonly DialogVotesBlockVM DialogVotesBlockVM;

	public readonly List<NetPlayer> VotedPlayers = new List<NetPlayer>(new List<NetPlayer>());

	private readonly ReactiveProperty<bool> m_WasChoose = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<TooltipBaseTemplate> m_AnswerTooltip = new ReactiveProperty<TooltipBaseTemplate>();

	private readonly ReactiveCommand<Unit> m_VotedPlayersChanged = new ReactiveCommand<Unit>();

	private bool m_ChooseCharacterInit;

	public List<PlayerInfo> AnswerVotes { get; private set; } = new List<PlayerInfo>();


	private static GameDialogsSettings DialogSettings => SettingsRoot.Game.Dialogs;

	private static DialogType DialogType => Game.Instance.Controllers.DialogController.Dialog.Type;

	public bool IsBookEvent => DialogType == DialogType.Book;

	public string AssetGuid => Answer.AssetGuid;

	public string BindingDisplayText
	{
		get
		{
			if (!m_TextByBinding.IsNullOrEmpty())
			{
				return m_TextByBinding;
			}
			int index = Index;
			return index.ToString();
		}
	}

	public string AnswerRawText => Answer.DisplayText;

	public bool CanSelect => Answer.CanSelect();

	public bool IsAlreadySelected => Game.Instance.DialogState.SelectedAnswersContains(Answer);

	public bool IsCurrentUnselectedWithNewAnswers
	{
		get
		{
			if (!m_DialogController.LocalSelectedAnswers.Contains(Answer))
			{
				return m_DialogController.HasNextUnselectedAnswers(Answer);
			}
			return false;
		}
	}

	public ReadOnlyReactiveProperty<bool> WasChoose => m_WasChoose;

	public ReadOnlyReactiveProperty<TooltipBaseTemplate> AnswerTooltip => m_AnswerTooltip;

	public bool HasTooltipData
	{
		get
		{
			if (!Answer.HasConditions && !Answer.HasExchangeData && !HasVisibleSkillChecks && !HasVisibleSkillCheckRequirement && !HasRelatedDetectiveItems && !HasVisibleConvictionShifts)
			{
				return HasCloseCaseData;
			}
			return true;
		}
	}

	public bool HasConditions => Answer.HasConditions;

	public bool HasVisibleExchangeData => Answer.HasExchangeData;

	public bool HasRelatedDetectiveItems => Answer.GetRelatedCaseItems().Any();

	public bool HasCloseCaseData => Answer.GetCloseCaseData().Any();

	public string ExchangeIDText => Answer.AssetGuid;

	public bool HasVisibleSkillChecks
	{
		get
		{
			if (!HasConditions && (bool)DialogSettings.ShowSkillcheckDC)
			{
				return SkillChecks.Any();
			}
			return false;
		}
	}

	public bool HasVisibleSkillCheckRequirement
	{
		get
		{
			if (!HasConditions && (bool)DialogSettings.ShowSkillcheckResult)
			{
				return Answer.HasSkillCheckRequirement;
			}
			return false;
		}
	}

	public ShowCheck AnswerSkillcheckRequirement => Answer?.ShowCheck;

	public List<SkillCheckDC> AllSkillChecks => SkillChecks.Concat(SkillCheckRequirements).ToList();

	public bool HasVisibleConvictionRequirements
	{
		get
		{
			if (!HasConditions && (bool)DialogSettings.ShowAlignmentRequirements)
			{
				return Answer.SelectConditions.Conditions.Any((Condition c) => c is HasAlignment);
			}
			return false;
		}
	}

	public bool ShowSpoiler
	{
		get
		{
			if (!Answer.CanSelect())
			{
				return HasVisibleConvictionRequirements;
			}
			return false;
		}
	}

	public bool HasVisibleConvictionShifts
	{
		get
		{
			if (!HasConditions && (bool)DialogSettings.ShowAlignmentShiftsInAnswer)
			{
				return Answer.AlignmentShifts.Any();
			}
			return false;
		}
	}

	public Observable<Unit> VotedPlayersChanged => m_VotedPlayersChanged;

	public BlueprintScriptableObject Blueprint => Answer;

	public AnswerVM(BlueprintAnswer answer, DialogController dialogController, int index)
	{
		Answer = answer;
		SkillChecks = Answer.CollectNextCueSkillChecks();
		SkillCheckRequirements = Answer.CollectRequiredSkillChecks();
		m_DialogController = dialogController;
		Index = index;
		BindingName = $"DialogChoice{Index}";
		m_TextByBinding = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(BindingName));
		IsSystem = Answer.IsSystem();
		m_ChooseCharacterInit = false;
		try
		{
			SetupTooltip();
		}
		catch (Exception ex)
		{
			PFLog.UI.Error(ex);
		}
		try
		{
			DialogVotesBlockVM = new DialogVotesBlockVM().AddTo(this);
		}
		catch (Exception ex2)
		{
			PFLog.UI.Error(ex2);
		}
		EventBus.Subscribe(this).AddTo(this);
	}

	public void OnChooseAnswer()
	{
		if (!CanSelect || m_ChooseCharacterInit || !UtilityNet.IsControlMainCharacterWithWarning(needSignalHowToPing: true))
		{
			return;
		}
		if (Answer.CharacterSelection.SelectionType == CharacterSelection.Type.Manual)
		{
			EventBus.RaiseEvent(delegate(IBookEventUIHandler h)
			{
				h.HandleChooseCharacter(Answer);
			});
		}
		else if (m_DialogController != null)
		{
			Game.Instance.GameCommandQueue.DialogAnswer(m_DialogController.CurrentCueUpdateTick, Answer.AssetGuid);
		}
		ChoosedAnswer.Value = Answer.Text;
		m_WasChoose.Value = true;
	}

	public void HandleChooseCharacter(BlueprintAnswer answer)
	{
		m_ChooseCharacterInit = true;
	}

	public void HandleChooseCharacterEnd()
	{
		m_ChooseCharacterInit = false;
	}

	public void PingAnswerHover(bool hover)
	{
		if (UtilityNet.IsControlMainCharacter())
		{
			try
			{
				PhotonManager.Ping.PingDialogAnswer(Answer.AssetGuid, hover);
			}
			catch (Exception arg)
			{
				PFLog.Net.Error($"Ping in dialog error {arg}");
				throw;
			}
		}
	}

	public void HandleDialogAnswerHover(string answer, bool hover)
	{
		if (!UtilityNet.IsControlMainCharacter() && !(answer != Answer.AssetGuid))
		{
			m_WasChoose.Value = hover;
		}
	}

	public void HandleDialogAnswerVote(NetPlayer player, string answer)
	{
		if (answer != Answer.AssetGuid)
		{
			if (VotedPlayers.Contains(player))
			{
				VotedPlayers.Remove(player);
				UpdateAnswerVotes();
			}
		}
		else if (VotedPlayers.Contains(player))
		{
			VotedPlayers.Remove(player);
			UpdateAnswerVotes();
		}
		else
		{
			CoopSounds.Instance.Pings.DialogVotePing.Play();
			VotedPlayers.Add(player);
			UpdateAnswerVotes();
		}
	}

	private void UpdateAnswerVotes()
	{
		if (VotedPlayers == null || PhotonManager.Instance == null)
		{
			return;
		}
		ReadonlyList<PlayerInfo> activePlayers = PhotonManager.Instance.ActivePlayers;
		AnswerVotes.Clear();
		AnswerVotes = VotedPlayers.Select((NetPlayer player) => activePlayers.FirstOrDefault((PlayerInfo p) => player.Index == p.NetPlayer.Index)).ToList();
		m_VotedPlayersChanged.Execute(Unit.Default);
	}

	private void SetupTooltip()
	{
		if (SkillChecks.Count > 0)
		{
			m_AnswerTooltip.Value = new TooltipTemplateSkillCheckDC(SkillChecks);
		}
		else
		{
			m_AnswerTooltip.Value = null;
		}
	}

	public bool TryDoCoopPing()
	{
		return PhotonManager.Ping.CheckPingCoop(delegate
		{
			PhotonManager.Ping.PingDialogAnswerVote(Answer.AssetGuid);
		});
	}

	public void DebugAnswer()
	{
		List<BlueprintAnswer> list = m_DialogController.CollectNextAnswers(Answer);
		bool flag = m_DialogController.HasNextUnselectedAnswers(Answer);
		Debug.Log($"Selected prev {IsAlreadySelected}, selected this turn {m_DialogController.LocalSelectedAnswers.Contains(Answer)} \nnext count {list.Count} ({list.Count((BlueprintAnswer a) => !Game.Instance.DialogState.SelectedAnswersContains(a))}) has unselected {flag}");
	}
}
