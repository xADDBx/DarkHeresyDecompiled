using System;
using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Replay;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.UI;
using R3;
using TMPro;

namespace Kingmaker.Code.UI.MVVM;

public class CommonContext : ViewModel, IDialogMessageBoxUIHandler, ISubscriber, IGameModeHandler, ITurnBasedModeHandler, ITurnBasedModeStartHandler, IAreaHandler, IAdditiveAreaSwitchHandler, ILootInteractionHandler, ISubscriber<IBaseUnitEntity>, IBugReportUIHandler
{
	private readonly ReactiveProperty<MessageBoxVM> m_MessageBoxVM = new ReactiveProperty<MessageBoxVM>();

	private readonly ReactiveProperty<BugReportVM> m_BugReportVM = new ReactiveProperty<BugReportVM>();

	private readonly ReactiveProperty<SubtitleVM> m_SubtitleVM = new ReactiveProperty<SubtitleVM>();

	private readonly Queue<MessageBoxVM> m_MessageQueue = new Queue<MessageBoxVM>();

	public ReadOnlyReactiveProperty<MessageBoxVM> MessageBoxVM => m_MessageBoxVM;

	public ReadOnlyReactiveProperty<BugReportVM> BugReportVM => m_BugReportVM;

	public ReadOnlyReactiveProperty<SubtitleVM> SubtitleVM => m_SubtitleVM;

	public CommonContext(ReactiveProperty<MessageBoxVM> messageBoxVM, ReactiveProperty<BugReportVM> bugReportVM, ReactiveProperty<SubtitleVM> subtitleVM, ReactiveProperty<TutorialVM> tutorialVM)
	{
		m_MessageBoxVM = messageBoxVM;
		m_BugReportVM = bugReportVM;
		m_SubtitleVM = subtitleVM;
		m_SubtitleVM.Value = new SubtitleVM().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleOpen(string messageText, DialogMessageBoxType boxType = DialogMessageBoxType.Message, Action<DialogMessageBoxButton> onClose = null, Action<TMP_LinkInfo> onLinkInvoke = null, string yesLabel = null, string noLabel = null, Action<string> onTextResult = null, string inputText = null, string inputPlaceholder = null, int waitTime = 0, uint maxInputTextLength = uint.MaxValue, ReadOnlyReactiveProperty<float> loadingProgress = null, Observable<Unit> loadingProgressCloseTrigger = null)
	{
		if (!RootUIContext.Instance.IsLoadingScreen)
		{
			MessageBoxVM messageBoxVM = new MessageBoxVM(messageText, boxType, onClose, onLinkInvoke, yesLabel, noLabel, onTextResult, inputText, inputPlaceholder, waitTime, DisposeMessageBox, loadingProgress, loadingProgressCloseTrigger);
			if (MessageBoxVM.CurrentValue == null)
			{
				m_MessageBoxVM.Value = messageBoxVM;
			}
			else
			{
				m_MessageQueue.Enqueue(messageBoxVM);
			}
		}
	}

	public void HandleClose()
	{
		DisposeMessageBox();
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.CutsceneGlobalMap || gameMode == GameModeType.StarSystem || gameMode == GameModeType.Dialog)
		{
			ForceDisposeAllFullscreen();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		ForceDisposeAllFullscreen();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		ForceDisposeAllFullscreen();
	}

	public void OnAdditiveAreaDidActivated()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			ForceDisposeAllFullscreen();
		}
	}

	void ITurnBasedModeStartHandler.HandleTurnBasedModeStarted()
	{
		ForceDisposeAllFullscreen();
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
	}

	public void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null)
	{
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		ForceDisposeAllFullscreen();
	}

	public void HandleBugReportOpen(bool showBugReportOnly)
	{
	}

	public void HandleBugReportCanvasHotKeyOpen()
	{
	}

	public void HandleBugReportShow()
	{
		m_BugReportVM.Value = new BugReportVM().AddTo(this);
		if (!Kingmaker.Replay.Replay.IsActive && !NetworkingManager.IsActive)
		{
			Game.Instance.StartMode(GameModeType.BugReport);
		}
	}

	public void HandleBugReportHide()
	{
		BugReportVM.CurrentValue?.Dispose();
		m_BugReportVM.Value = null;
		Game.Instance.StopMode(GameModeType.BugReport);
	}

	public void HandleUIElementFeature(string featureName)
	{
	}

	public void HandleCrushDumpReport()
	{
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		DisposeMessageBox();
		DisposeMessageQueue();
		ForceDisposeAllFullscreen();
	}

	private void DisposeMessageBox()
	{
		MessageBoxVM.CurrentValue?.Dispose();
		m_MessageBoxVM.Value = null;
		if (m_MessageQueue.Count > 0)
		{
			MessageBoxVM value = m_MessageQueue.Dequeue();
			m_MessageBoxVM.Value = value;
		}
	}

	private void DisposeMessageQueue()
	{
		while (m_MessageQueue.Count > 0)
		{
			m_MessageQueue.Dequeue().Dispose();
		}
	}

	private void ForceDisposeAllFullscreen()
	{
		DisposeMessageBox();
		HandleBugReportHide();
	}
}
