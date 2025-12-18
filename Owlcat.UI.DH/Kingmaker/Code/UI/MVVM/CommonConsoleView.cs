using System;
using System.Collections;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.Vendor;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.GameModes;
using Kingmaker.ResourceLinks;
using Kingmaker.UI.Common;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Selection;
using Kingmaker.UI.Workarounds;
using Owlcat.UI;
using R3;
using Rewired;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class CommonConsoleView : CommonBaseView
{
	[SerializeField]
	private UIVisibilityView m_UIVisibilityCommonView;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityBugReportView;

	[SerializeField]
	private TextMeshProUGUI m_UIVisibilityText;

	[SerializeField]
	private FadeAnimator m_UIVisibilityFadeAnimator;

	[Space]
	[SerializeField]
	private CounterWindowConsoleView m_CounterWindowConsoleView;

	[SerializeField]
	private ContextMenuConsoleView m_ContextMenuConsoleView;

	[SerializeField]
	private GamepadDisconnectedInGamepadModeWindowView m_GamepadDisconnectedInGamepadModeWindowView;

	[SerializeField]
	private MultiplySelection m_MultiplySelection;

	[SerializeField]
	private UIViewLinkTemp<SettingsConsoleView, SettingsVM> m_SettingsView;

	[SerializeField]
	private FadeView m_FadeView;

	[SerializeField]
	private UIViewLinkTemp<NetLobbyConsoleView, NetLobbyVM> m_NetLobbyConsoleView;

	[SerializeField]
	private UIViewLinkTemp<NetRolesConsoleView, NetRolesVM> m_NetRolesConsoleView;

	[SerializeField]
	private UIViewLinkTemp<DlcManagerConsoleView, DlcManagerVM> m_DlcManagerConsoleView;

	private Coroutine m_DisappearAnimationCoroutine;

	private IDisposable m_EscHotkey;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityView;

	[Space]
	[SerializeField]
	private RootConsoleView mRootConsoleView;

	[SerializeField]
	private UIViewLinkTemp<VendorBaseScreenView, VendorBaseScreenVM> m_VendorConsoleViewLink;

	[SerializeField]
	private UIViewLinkTemp<VendorSelectingWindowConsoleView, VendorSelectingWindowVM> m_VendorSelectingWindowContextConsoleView;

	[SerializeField]
	private SurfaceHUDConsoleView m_SurfaceHUDConsoleView;

	[SerializeField]
	private UIViewLinkTemp<CreditsConsoleView, CreditsVM> m_CreditsConsoleView;

	[SerializeField]
	private GameOverConsoleView m_GameOverConsoleView;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityDynamicView;

	[SerializeField]
	private CanvasGroup m_DynamicCanvasGroup;

	[SerializeField]
	private UIVisibilityView m_UIVisibilityPointerView;

	[SerializeField]
	private VariativeInteractionConsoleView m_VariativeInteractionView;

	[SerializeField]
	private SurfaceOvertipsConsoleView m_SurfaceOvertipsView;

	[SerializeField]
	private ConsoleCursor m_ConsoleCursor;

	[Header("Canvas scaler")]
	[SerializeField]
	private CanvasScalerWorkaround m_DynamicCanvasScalerWorkaround;

	[SerializeField]
	private RectTransform m_StaticCanvasRT;

	private static bool GamePadConfirm
	{
		set
		{
			if (Game.Instance.Controllers.ClickEventsController != null)
			{
				Game.Instance.Controllers.ClickEventsController.GamePadConfirm = value;
			}
		}
	}

	private static bool GamePadDecline
	{
		set
		{
			if (Game.Instance.Controllers.ClickEventsController != null)
			{
				Game.Instance.Controllers.ClickEventsController.GamePadDecline = value;
			}
		}
	}

	public override void Initialize()
	{
		base.Initialize();
		m_GamepadDisconnectedInGamepadModeWindowView.Initialize();
		m_UIVisibilityFadeAnimator.Initialize();
		m_SurfaceHUDConsoleView.Initialize();
		m_SurfaceOvertipsView.Initialize();
		m_ConsoleCursor.Initialize(m_DynamicCanvasScalerWorkaround);
	}

	protected override void BindViewImplementation()
	{
		m_UIVisibilityCommonView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityBugReportView.Bind(base.ViewModel.UIVisibilityVM);
		m_FadeView.Bind(base.ViewModel.FadeVM);
		AddDisposable(base.ViewModel.ContextMenuVM.Subscribe(m_ContextMenuConsoleView.Bind));
		AddDisposable(base.ViewModel.CounterWindowVM.Subscribe(m_CounterWindowConsoleView.Bind));
		AddDisposable(base.ViewModel.NetLobbyVM.Subscribe(m_NetLobbyConsoleView.Bind));
		AddDisposable(base.ViewModel.NetRolesVM.Subscribe(m_NetRolesConsoleView.Bind));
		AddDisposable(base.ViewModel.DlcManagerVM.Subscribe(m_DlcManagerConsoleView.Bind));
		AddDisposable(UIVisibilityState.VisibilityPreset.Skip(1).Subscribe(delegate
		{
			UIVisibilityChange();
		}));
		m_UIVisibilityView.Bind(base.ViewModel.UIVisibilityVM);
		AddDisposable(base.ViewModel.CreditsVM.Subscribe(m_CreditsConsoleView.Bind));
		AddDisposable(base.ViewModel.VendorVM.Subscribe(m_VendorConsoleViewLink.Bind));
		AddDisposable(base.ViewModel.VendorSelectingWindowVM.Subscribe(m_VendorSelectingWindowContextConsoleView.Bind));
		m_UIVisibilityDynamicView.Bind(base.ViewModel.UIVisibilityVM);
		m_UIVisibilityPointerView.Bind(base.ViewModel.UIVisibilityVM);
		AddDisposable(m_ConsoleCursor.Bind());
		AddDisposable(ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			OnUpdate();
		}));
	}

	private void OnUpdate()
	{
		if (m_DynamicCanvasGroup.blocksRaycasts != ConsoleCursor.Instance.IsActive)
		{
			m_DynamicCanvasGroup.blocksRaycasts = ConsoleCursor.Instance.IsActive;
		}
	}

	private void InitFormation(FormationConsoleView view)
	{
		view.GetComponent<DraggbleWindow>().SetParentRectTransform(m_StaticCanvasRT);
	}

	protected override void DestroyViewImplementation()
	{
		if (m_DisappearAnimationCoroutine != null)
		{
			StopCoroutine(m_DisappearAnimationCoroutine);
			m_DisappearAnimationCoroutine = null;
		}
		m_EscHotkey?.Dispose();
		m_EscHotkey = null;
	}

	private void UIVisibilityChange()
	{
		if (UIVisibilityState.VisibilityPresetIndex != 9)
		{
			if (m_EscHotkey == null)
			{
				m_EscHotkey = EscHotkeyManager.Instance.Subscribe(ResetUIVisibility);
			}
		}
		else
		{
			m_EscHotkey?.Dispose();
			m_EscHotkey = null;
		}
		m_UIVisibilityFadeAnimator.AppearAnimation();
		if (m_DisappearAnimationCoroutine != null)
		{
			StopCoroutine(m_DisappearAnimationCoroutine);
		}
		m_DisappearAnimationCoroutine = StartCoroutine(DisappearAnimationCoroutine());
		string text = UIStrings.Instance.CommonTexts.UIVisibility.Text;
		m_UIVisibilityText.text = text + "<br>" + UIVisibilityState.VisibilityPresetIndex + "/" + 9;
	}

	private void ResetUIVisibility()
	{
		UIVisibilityState.ShowAllUI();
	}

	private IEnumerator DisappearAnimationCoroutine()
	{
		yield return new WaitForSecondsRealtime(1f);
		m_UIVisibilityFadeAnimator.DisappearAnimation();
		m_DisappearAnimationCoroutine = null;
	}

	protected override void CreateBaseInputImpl(InputLayer baseInputLayer)
	{
		AddBaseInput(baseInputLayer);
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			DelayedQuestNotificationDecline();
		}, 9, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			DelayedQuestNotificationJournal();
		}, 17));
	}

	protected override void CreateMainInputImpl(InputLayer mainInputLayer)
	{
		AddDisposable(SurfaceMainInputLayer.AddButton(delegate
		{
			OnShowEscMenu();
		}, 16, InputActionEventType.ButtonJustReleased));
		AddDisposable(mainInputLayer.AddButton(OnInteract, 8, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(mainInputLayer.AddButton(OnDecline, 9, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(mainInputLayer.AddButton(delegate
		{
			OnNextInteractable(combat: false);
		}, 15, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddDisposable(mainInputLayer.AddButton(delegate
		{
			OnPrevInteractable(combat: false);
		}, 14, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddMainInput(mainInputLayer);
		SubscribeToPointerClicks(mainInputLayer);
	}

	protected override void CreateCombatInputImpl(InputLayer combatInputLayer)
	{
		AddDisposable(SurfaceCombatInputLayer.AddButton(delegate
		{
			OnShowEscMenu();
		}, 16, InputActionEventType.ButtonJustReleased));
		HandleBeginCombat();
		AddDisposable(combatInputLayer.AddButton(delegate
		{
			OnNextInteractable(combat: true);
		}, 15, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddDisposable(combatInputLayer.AddButton(delegate
		{
			OnPrevInteractable(combat: true);
		}, 14, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddCombatInput(combatInputLayer);
		SubscribeToPointerClicks(combatInputLayer);
	}

	private void SubscribeToPointerClicks(InputLayer inputLayer)
	{
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadConfirm = true;
		}, 8));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadConfirm = false;
		}, 8, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadConfirm = false;
		}, 8, InputActionEventType.ButtonLongPressJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadDecline = true;
		}, 9));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadDecline = false;
		}, 9, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			GamePadDecline = false;
		}, 9, InputActionEventType.ButtonLongPressJustReleased));
	}

	private void OnInteract(InputActionEventData eventData)
	{
		SurfaceMainInputLayer.OnInteract();
	}

	private void DelayedQuestNotificationDecline()
	{
		DelayedInvoker.InvokeInFrames(OnQuestNotificationDecline, 1);
	}

	private void DelayedQuestNotificationJournal()
	{
		DelayedInvoker.InvokeInFrames(OnQuestNotificationJournal, 1);
	}

	private void OnQuestNotificationDecline()
	{
		if (base.ViewModel.IsInQuestNotification)
		{
			base.ViewModel.QuestNotificationForceClose();
		}
	}

	private void OnQuestNotificationJournal()
	{
		bool flag = RootUIContext.Instance.FullScreenUIType == FullScreenUIType.Journal;
		bool flag2 = Game.Instance.CurrentModeType == GameModeType.Cutscene;
		bool flag3 = Game.Instance.CurrentModeType == GameModeType.Dialog;
		if (base.ViewModel.IsInQuestNotification && !flag && !flag2 && !flag3)
		{
			base.ViewModel.OpenJournal();
		}
	}

	private void OnDecline(InputActionEventData obj)
	{
		_ = base.ViewModel.IsInQuestNotification;
	}

	private void OnNextInteractable(bool combat)
	{
		if (combat)
		{
			SurfaceCombatInputLayer.OnNextInteractable();
		}
		else
		{
			SurfaceMainInputLayer.OnNextInteractable();
		}
	}

	private void OnPrevInteractable(bool combat)
	{
		if (combat)
		{
			SurfaceCombatInputLayer.OnPrevInteractable();
		}
		else
		{
			SurfaceMainInputLayer.OnPrevInteractable();
		}
	}

	public void AddBaseInput(InputLayer inputLayer)
	{
		m_SurfaceHUDConsoleView.AddBaseInput(inputLayer);
	}

	public void AddMainInput(InputLayer inputLayer)
	{
		m_SurfaceHUDConsoleView.AddMainInput(inputLayer);
	}

	public void AddCombatInput(InputLayer inputLayer)
	{
		m_SurfaceHUDConsoleView.AddCombatInput(inputLayer);
	}

	public void OnShowEscMenu()
	{
		if (!Game.Instance.TutorialSystem.HasShownData && !(Game.Instance.CurrentModeType == GameModeType.GameOver))
		{
			base.ViewModel.HandleShowEscMenu();
		}
	}

	public void HandleBeginCombat()
	{
		m_SurfaceHUDConsoleView.SwitchPartySelector(isEnabled: false);
	}
}
