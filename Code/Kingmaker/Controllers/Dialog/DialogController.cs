using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.DialogSystem;
using Kingmaker.DialogSystem.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.Localization;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.QA;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Sound;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Controllers.Dialog;

public class DialogController : IControllerTick, IController, IControllerStart, IControllerStop, IControllerReset, IAreaHandler, ISubscriber, IDialogControllerStartScheduledDialogImmediately
{
	private static readonly LogChannel Logger = PFLog.Dialog;

	private readonly DialogEngine m_Engine;

	private readonly DialogDirector m_Director;

	private DialogContext m_Context;

	private DialogHistory m_History;

	private readonly DialogAbnormalTerminationChecker m_AbnormalTerminationChecker;

	public int CurrentCueUpdateTick { get; private set; }

	public bool DialogStopScheduled { get; private set; }

	public Vector3 DialogPosition
	{
		get
		{
			return m_Context?.DialogPosition ?? Vector3.zero;
		}
		set
		{
			if (m_Context != null)
			{
				m_Context.DialogPosition = value;
			}
		}
	}

	[CanBeNull]
	public BaseUnitEntity FirstSpeaker => m_Context?.FirstSpeaker;

	[CanBeNull]
	public BaseUnitEntity CurrentSpeaker => m_Context?.CurrentSpeaker;

	[CanBeNull]
	public MapObjectEntity MapObject => m_Context?.MapObject;

	[CanBeNull]
	public BaseUnitEntity ActingUnit => m_Context?.ActingUnit;

	[NotNull]
	public IEnumerable<BaseUnitEntity> InvolvedUnits => (m_Context?.InvolvedUnits).EmptyIfNull();

	[CanBeNull]
	public string CurrentSpeakerName => m_Context?.CurrentSpeakerName;

	[CanBeNull]
	public BlueprintUnit CurrentSpeakerBlueprint => m_Context?.CurrentSpeakerBlueprint;

	public CueShowData CurrentCueShowData => m_Engine.CurrentCueShowData;

	[CanBeNull]
	public BlueprintDialog Dialog => m_Engine?.Dialog;

	public IEnumerable<BlueprintAnswer> Answers => m_Engine.Answers;

	[NotNull]
	public HashSet<BlueprintCueBase> LocalShownCues => m_Engine.LocalShownCues;

	[NotNull]
	public HashSet<BlueprintAnswer> LocalSelectedAnswers => m_Engine.LocalSelectedAnswers;

	[NotNull]
	public HashSet<BlueprintAnswersList> LocalShownAnswerLists => m_Engine.LocalShownAnswerLists;

	[NotNull]
	public HashSet<BlueprintCheck> LocalPassedChecks => m_Engine.LocalPassedChecks;

	[NotNull]
	public HashSet<BlueprintCheck> LocalFailedChecks => m_Engine.LocalFailedChecks;

	public BlueprintCue CurrentCue => m_Engine.CurrentCue;

	public bool CuePlayScheduled => m_Engine.CuePlayScheduled;

	[NotNull]
	public IEnumerable<IDialogShowData> History
	{
		get
		{
			IEnumerable<IDialogShowData> enumerable = m_History?.History;
			return enumerable ?? Enumerable.Empty<IDialogShowData>();
		}
	}

	public static DialogData SetupDialogWithUnit([NotNull] BlueprintDialog dialog, [NotNull] BaseUnitEntity unit, [CanBeNull] BaseUnitEntity initiator = null)
	{
		return new DialogData
		{
			Dialog = dialog,
			Initiator = initiator.FromBaseUnitEntity(),
			Unit = unit.FromBaseUnitEntity(),
			MapObject = null,
			CustomSpeakerName = null
		};
	}

	public static DialogData SetupDialogWithMapObject([NotNull] BlueprintDialog dialog, [NotNull] MapObjectEntity mapObject, [CanBeNull] LocalizedString speakerName, [CanBeNull] BaseUnitEntity initiator = null)
	{
		return new DialogData
		{
			Dialog = dialog,
			Initiator = initiator.FromBaseUnitEntity(),
			Unit = UnitReference.NullUnitReference,
			MapObject = mapObject,
			CustomSpeakerName = speakerName
		};
	}

	public static DialogData SetupDialogWithoutTarget([NotNull] BlueprintDialog dialog, [CanBeNull] LocalizedString speakerName, [CanBeNull] BaseUnitEntity initiator = null)
	{
		return new DialogData
		{
			Dialog = dialog,
			Initiator = initiator.FromBaseUnitEntity(),
			Unit = UnitReference.NullUnitReference,
			MapObject = null,
			CustomSpeakerName = speakerName
		};
	}

	public DialogController()
	{
		m_Engine = new DialogEngine();
		m_Director = new DialogDirector();
		m_AbnormalTerminationChecker = new DialogAbnormalTerminationChecker();
		m_Engine.ExitedCue += HandleExitedCue;
		m_Engine.EnteringCue += HandleEnteringCue;
		m_Engine.ShowingCue += HandleShowingCue;
		m_Engine.ShownBookPage += HandleShownBookPage;
		m_Engine.StoppingDialog += HandleStoppingDialog;
		m_Engine.SelectingAnswer += HandleSelectingAnswer;
		m_Engine.SelectedAnswer += HandleSelectedAnswer;
	}

	public TickType GetTickType()
	{
		return TickType.Simulation;
	}

	public void Tick()
	{
		if (Dialog == null)
		{
			StopDialog();
		}
		else
		{
			m_Engine.TryPlayNextCue();
		}
		DialogStopScheduled = false;
	}

	public void StartDialog(DialogData data)
	{
		Logger.Log(data.Dialog, "Requested dialog start {0}", data.Dialog);
		Game.Instance.GameCommandQueue.ScheduleDialogStart(data);
	}

	public void StopDialog()
	{
		if (BuildModeUtility.IsDevelopment)
		{
			m_AbnormalTerminationChecker.Answers = Answers.ToList();
			if (!m_AbnormalTerminationChecker.Check(out var errorMessage))
			{
				string text = m_AbnormalTerminationChecker.Dialog?.name ?? "<null>";
				PFLog.History.Dialog.ErrorWithReport("Dialog '" + text + "' was unexpectedly terminated. See some reasons below.\n\r" + errorMessage);
			}
		}
		if (DialogStopScheduled)
		{
			return;
		}
		DialogStopScheduled = true;
		BlueprintDialog dialog = Dialog;
		m_Director.ResetDialogScene();
		if (!Game.Instance.GameCommandQueue.ContainsCommand((StartScheduledDialogCommand _) => true))
		{
			Game.Instance.StopMode(GameModeType.Dialog);
		}
		try
		{
			if (dialog != null)
			{
				EventBus.RaiseEvent(delegate(IDialogInteractionHandler h)
				{
					h.StopDialogInteraction(dialog);
				});
			}
		}
		catch (Exception ex)
		{
			PFLog.Default.Exception(Dialog, ex);
		}
		m_Engine.FinishDialog();
	}

	public void SelectAnswer(string answerGuid)
	{
		m_Engine.SelectAnswer(answerGuid);
	}

	public void SelectAnswer(BlueprintAnswer answer, BaseUnitEntity manualUnitSelection = null)
	{
		m_Engine.SelectAnswer(answer, manualUnitSelection);
	}

	public bool HasNextUnselectedAnswers(BlueprintAnswer currentAnswer)
	{
		return CollectNextAnswers(currentAnswer).Any((BlueprintAnswer a) => !Game.Instance.DialogState.SelectedAnswersContains(a));
	}

	public List<BlueprintAnswer> CollectNextAnswers(BlueprintAnswer currentAnswer)
	{
		List<BlueprintAnswer> list = new List<BlueprintAnswer>();
		if (!(currentAnswer.NextCue.Select() is BlueprintCue blueprintCue))
		{
			return list;
		}
		foreach (BlueprintAnswerBaseReference answer in blueprintCue.Answers)
		{
			if (answer.Blueprint != currentAnswer && (!(answer.Blueprint is BlueprintAnswersList blueprintAnswersList) || !blueprintAnswersList.Answers.Dereference().Contains(currentAnswer)))
			{
				CollectAnswersRecursive(answer.Blueprint, list);
			}
		}
		list.RemoveAll((BlueprintAnswer a) => !a.CanShow() || !a.CanSelect());
		return list;
	}

	void IDialogControllerStartScheduledDialogImmediately.StartScheduledDialogImmediately(DialogData dialog)
	{
		StartScheduledDialogImmediately(dialog);
	}

	private void StartScheduledDialogImmediately([NotNull] DialogData scheduled)
	{
		if (scheduled == null)
		{
			throw new ArgumentException("scheduled is null", "scheduled");
		}
		m_Engine.TryPlayNextCue();
		BlueprintDialog dialog = scheduled.Dialog;
		DialogContext dialogContext = new DialogContext(scheduled);
		bool flag = true;
		if (Game.Instance.Player.GameOverReason.HasValue)
		{
			flag = false;
			Logger.Error("Trying to start dialog when the game is over");
		}
		if (Dialog != null)
		{
			flag = false;
			Logger.Error("Trying to start dialog twice. Current {0}, New {1}", Dialog, dialog);
		}
		DialogDebug.Init(dialog);
		if (!dialog.Conditions.Check(Dialog))
		{
			flag = false;
			DialogDebug.Add(dialog, "start conditions failed", Color.red);
		}
		if (dialog.FirstCue.Select() == null)
		{
			flag = false;
			DialogDebug.Add(Dialog, "could not select first cue", Color.red);
		}
		if (IsSpeakerBusyInCutscene(dialogContext.FirstSpeaker, out var cutscene))
		{
			flag = false;
			DialogDebug.Add(dialog, $"first speaker {dialogContext.FirstSpeaker.Blueprint} is busy in cutscene {cutscene.Cutscene} ({cutscene.Cutscene.AssetGuid})", Color.red);
		}
		using ((dialogContext.FirstSpeaker != null) ? ContextData<ClickedUnitData>.Request().Setup(dialogContext.FirstSpeaker) : null)
		{
			using ((dialogContext.MapObject != null) ? ContextData<MechanicEntityData>.Request().Setup(dialogContext.MapObject) : null)
			{
				if (!flag)
				{
					dialog.ReplaceActions.Run();
					EventBus.RaiseEvent(delegate(IDialogFinishHandler h)
					{
						h.HandleDialogFinished(dialog, success: false);
					});
					return;
				}
				dialog.StartActions.Run();
			}
		}
		Game.Instance.Controllers.ProjectileController.Clear();
		Clear();
		m_Context = dialogContext;
		m_History = new DialogHistory(m_Context, Game.Instance.Player.MainCharacterEntity);
		m_Engine.StartDialog(m_Context, Game.Instance.DialogState);
		if (Game.Instance.CurrentModeType != GameModeType.Dialog)
		{
			Game.Instance.StartMode(GameModeType.Dialog);
		}
		EventBus.RaiseEvent(delegate(IDialogInteractionHandler h)
		{
			h.StartDialogInteraction(Dialog);
		});
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleCloseAll();
		});
		m_AbnormalTerminationChecker.Dialog = Dialog;
	}

	private static bool IsSpeakerBusyInCutscene(BaseUnitEntity speakerEntity, out CutscenePlayerData cutscene)
	{
		if (speakerEntity == null)
		{
			cutscene = null;
			return false;
		}
		cutscene = CutsceneControlledUnit.GetControllingPlayer(speakerEntity);
		if (cutscene != null)
		{
			return cutscene.Cutscene.ForbidDialogs;
		}
		return false;
	}

	private void Clear()
	{
		m_Context = null;
		m_History = null;
		m_Director.Clear();
		m_Engine.Clear();
	}

	private void HandleExitedCue(BlueprintCue cue)
	{
		m_Director.HandleExitedCue(cue);
	}

	private void HandleEnteringCue(BlueprintCue cue)
	{
		CurrentCueUpdateTick = Game.Instance.RealTimeController.CurrentNetworkTick;
		m_Director.HandleEnteringCue(cue);
	}

	private void HandleShowingCue(CueShowData cueShowData)
	{
		SoundState.Instance.StopDialog();
		SoundState.Instance.StartDialog();
		if (!m_Engine.IsPlayingBookPage)
		{
			m_History.AddCue(cueShowData.BlueprintCue);
			EventBus.RaiseEvent(delegate(IDialogCueHandler h)
			{
				h.HandleOnCueShow(cueShowData);
			});
		}
		m_AbnormalTerminationChecker.CurrentCue = cueShowData.BlueprintCue;
	}

	private void HandleShownBookPage(BlueprintBookPage page, List<CueShowData> bookPageCues, List<BlueprintAnswer> answers)
	{
		EventBus.RaiseEvent(delegate(IBookPageHandler h)
		{
			h.HandleOnBookPageShow(page, bookPageCues, answers);
		});
	}

	private void HandleStoppingDialog(IDialogContext context)
	{
		if (m_Context == context)
		{
			StopDialog();
		}
	}

	private void HandleSelectingAnswer(BlueprintAnswer answer)
	{
		m_History.AddAnswer(answer);
	}

	private void HandleSelectedAnswer(BlueprintAnswer answer)
	{
		EventBus.RaiseEvent(delegate(ISelectAnswerHandler h)
		{
			h.HandleSelectAnswer(answer);
		});
		m_AbnormalTerminationChecker.LastSelectedAnswer = answer;
	}

	private void CollectAnswersRecursive(BlueprintAnswerBase answerBase, List<BlueprintAnswer> result)
	{
		if (answerBase == null)
		{
			return;
		}
		if (answerBase is BlueprintAnswersList blueprintAnswersList)
		{
			{
				foreach (BlueprintAnswerBaseReference answer in blueprintAnswersList.Answers)
				{
					CollectAnswersRecursive(answer, result);
				}
				return;
			}
		}
		if (answerBase is BlueprintAnswer item)
		{
			result.Add(item);
		}
	}

	void IControllerStart.OnStart()
	{
		ActiveEncounter.Current?.TryCompleteImmediately();
		m_Director.SetUpDialogScene(m_Context);
		foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
		{
			if (CutsceneControlledUnit.GetControllingPlayer(allUnit) == null)
			{
				allUnit.Commands.InterruptAllInterruptible();
			}
		}
		NetService.Instance.CancelCurrentCommands();
		EventBus.RaiseEvent(delegate(IDialogStartHandler h)
		{
			h.HandleDialogStarted(Dialog);
		});
	}

	void IControllerStop.OnStop()
	{
		m_Director.ResetDialogScene();
		SoundState.Instance.StopDialog();
		EventBus.RaiseEvent(delegate(IDialogFinishHandler h)
		{
			h.HandleDialogFinished(Dialog, success: true);
		});
		Clear();
	}

	void IControllerReset.OnReset()
	{
		Clear();
	}

	void IAreaHandler.OnAreaBeginUnloading()
	{
		if (Dialog == null)
		{
			Clear();
		}
	}

	void IAreaHandler.OnAreaDidLoad()
	{
	}
}
