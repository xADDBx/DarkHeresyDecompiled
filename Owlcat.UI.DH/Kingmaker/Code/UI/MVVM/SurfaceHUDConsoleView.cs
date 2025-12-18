using System;
using System.Collections;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Pointer;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

[Obsolete("Needs refactoring. Move everything to RootConsoleView.")]
public class SurfaceHUDConsoleView : ViewBase<SurfaceHUDVM>, IGameModeHandler, ISubscriber
{
	[SerializeField]
	private IngameMenuConsoleView m_IngameMenuConsoleView;

	[SerializeField]
	private PartyConsoleView m_PartyConsoleView;

	[SerializeField]
	private PartySelectorConsoleView m_PartySelectorConsoleView;

	[SerializeField]
	private ActionBarUnitView m_SurfaceCombatCurrentUnitView;

	[SerializeField]
	private ActionBarConsoleView m_ActionBarConsoleView;

	[SerializeField]
	private InitiativeTrackerView m_InitiativeTrackerView;

	[SerializeField]
	private InspectConsoleView m_InspectConsoleView;

	[SerializeField]
	private CombatStartWindowConsoleView m_CombatStartWindowView;

	[Header("CombatLog Settings")]
	[SerializeField]
	private CombatLogConsoleView m_CombatLogConsoleView;

	[SerializeField]
	protected MoveAnimator m_CombatLogPositionAnimator;

	[Header("Cutscene hints")]
	[SerializeField]
	private ConsoleHint m_SkipCutsceneHint;

	[SerializeField]
	private FadeAnimator m_SkipCutsceneHintHolderFade;

	private InputLayer m_CutSceneInputLayer;

	private InputBindStruct m_SkipCutsceneHintStruct;

	private IDisposable m_TurnUnitSubscription;

	private IDisposable m_SelectedUnitSubscription;

	private readonly ReactiveProperty<bool> m_IsSkipCutsceneHintActive = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsSkipCutsceneLongPressAnimActive = new ReactiveProperty<bool>();

	private IEnumerator m_HideCloseCutsceneHint;

	[Header("Main input hints")]
	[SerializeField]
	private ConsoleHint m_PauseGameHint;

	[SerializeField]
	private FadeAnimator m_AdditionalHintsContainer;

	private IDisposable m_DisappearTask;

	[SerializeField]
	private float m_HintsDisappearTime = 5f;

	[SerializeField]
	private ConsoleHint m_ChangeCameraRotateModeHint;

	[SerializeField]
	private ConsoleHint m_FocusOnCurrentUnitHint;

	[SerializeField]
	private ConsoleHint m_SwitchCursorHint;

	[SerializeField]
	private ConsoleHint m_OpenMapHint;

	[SerializeField]
	private ConsoleHint m_SwitchHighlightHint;

	[SerializeField]
	private ConsoleHint m_CoopRolesHint;

	[SerializeField]
	private ConsoleHint m_EscMenuHint;

	[Header("Combat input hints")]
	[SerializeField]
	private ConsoleHint m_FocusOnCurrentUnitCombatHint;

	[SerializeField]
	private ConsoleHint m_HighlightObjectsCombatHint;

	[SerializeField]
	private ConsoleHint m_EndTurnCombatHint;

	[SerializeField]
	private ConsoleHint m_StartBattleHint;

	[SerializeField]
	private ConsoleHint m_CoopRolesSurfaceCombatHint;

	[SerializeField]
	private ConsoleHint m_EscMenuCombatHint;

	[Header("Other")]
	[SerializeField]
	private Image m_NetRolesAttentionMark;

	[Header("Other")]
	[SerializeField]
	private Image m_NetRolesAttentionSurfaceCombatMark;

	private readonly ReactiveProperty<bool> m_IsPaused = new ReactiveProperty<bool>();

	private bool IsIngameMenuAllowed
	{
		get
		{
			if (!Game.Instance.IsModeActive(GameModeType.Dialog))
			{
				return !Game.Instance.TradeLogic.IsTrading;
			}
			return false;
		}
	}

	public void Initialize()
	{
		m_IngameMenuConsoleView.Initialize();
		m_PartyConsoleView.Initialize();
		m_PartySelectorConsoleView.Initialize();
		m_ActionBarConsoleView.Initialize();
		m_InspectConsoleView.Initialize();
	}

	protected override void BindViewImplementation()
	{
		AddDisposable(EventBus.Subscribe(this));
		m_PartyConsoleView.Bind(base.ViewModel.PartyVM);
		m_ActionBarConsoleView.Bind(base.ViewModel.ActionBarVM);
		m_InspectConsoleView.Bind(base.ViewModel.InspectVM);
		m_CombatLogConsoleView.Bind(base.ViewModel.CombatLogVM);
		AddDisposable(base.ViewModel.InitiativeTrackerVM.Subscribe(m_InitiativeTrackerView.Bind));
		AddDisposable(base.ViewModel.CombatStartWindowVM.Subscribe(m_CombatStartWindowView.Bind));
		AddDisposable(base.ViewModel.CurrentUnit.Subscribe(m_SurfaceCombatCurrentUnitView.Bind));
		AddDisposable(base.ViewModel.ActionBarVM.IsVisible.Subscribe(ActionBarVisibilityChanged));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(MainThreadDispatcher.LateUpdateAsObservable(), delegate
		{
			InternalUpdate();
		}));
		m_AdditionalHintsContainer.AppearAnimation();
		AddDisposable(m_IsPaused.Subscribe(OnPauseCanged));
		AddDisposable(base.ViewModel.PlayerHaveRoles.CombineLatest(base.ViewModel.NetFirstLoadState, (bool haveRoles, bool netFirstLoadState) => new { haveRoles, netFirstLoadState }).Subscribe(value =>
		{
			m_NetRolesAttentionMark.gameObject.SetActive(value.netFirstLoadState && !value.haveRoles);
			m_NetRolesAttentionSurfaceCombatMark.gameObject.SetActive(value.netFirstLoadState && !value.haveRoles);
		}));
	}

	public void ActionBarVisibilityChanged(bool state)
	{
		if (state)
		{
			m_CombatLogPositionAnimator.AppearAnimation();
		}
		else
		{
			m_CombatLogPositionAnimator.DisappearAnimation();
		}
	}

	private void OnPauseCanged(bool isPaused)
	{
		if (isPaused)
		{
			if (m_DisappearTask != null)
			{
				m_DisappearTask.Dispose();
				m_DisappearTask = null;
			}
			m_AdditionalHintsContainer.AppearAnimation();
		}
		else
		{
			m_DisappearTask = DelayedInvoker.InvokeInTime(delegate
			{
				m_AdditionalHintsContainer.DisappearAnimation();
			}, m_HintsDisappearTime);
		}
	}

	protected virtual void InternalUpdate()
	{
		if (!(Game.Instance.CurrentModeType != GameModeType.Cutscene) && !m_IsSkipCutsceneHintActive.Value && Input.anyKeyDown)
		{
			ShowSkipHint();
		}
	}

	private void ShowSkipHint()
	{
		m_IsSkipCutsceneHintActive.Value = true;
		m_IsSkipCutsceneLongPressAnimActive.Value = true;
	}

	public void AddBaseInput(InputLayer baseInputLayer)
	{
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchPartySelector(isEnabled: true);
		}, 12, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchPartySelector(isEnabled: false);
		}, 12, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchPartySelector(isEnabled: false);
		}, 12, InputActionEventType.ButtonLongPressJustReleased, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchIngameMenu(IsIngameMenuAllowed);
		}, 13, InputActionEventType.ButtonJustPressed, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchIngameMenu(isEnabled: false);
		}, 13, InputActionEventType.ButtonJustReleased, enableDefaultSound: false));
		AddDisposable(baseInputLayer.AddButton(delegate
		{
			SwitchIngameMenu(isEnabled: false);
		}, 13, InputActionEventType.ButtonLongPressJustReleased, enableDefaultSound: false));
	}

	public void AddMainInput(InputLayer inputLayer)
	{
		AddDisposable(m_ChangeCameraRotateModeHint.Bind(inputLayer.AddButton(ChangeCameraRotateMode, 19, ConsoleCursor.Instance.IsNotActiveProperty, InputActionEventType.ButtonJustReleased)));
		m_ChangeCameraRotateModeHint.SetLabel(UIStrings.Instance.HUDTexts.SwitchCameraMode);
		AddDisposable(m_FocusOnCurrentUnitHint.Bind(inputLayer.AddButton(FocusOnCurrentUnit, 19, ConsoleCursor.Instance.IsActiveProperty, InputActionEventType.ButtonJustReleased)));
		m_FocusOnCurrentUnitHint.SetLabel(UIStrings.Instance.HUDTexts.FocusOnCurrentUnit);
		AddDisposable(m_SwitchCursorHint.Bind(inputLayer.AddButton(delegate
		{
			SwitchCursor(inputLayer);
		}, 18, InputActionEventType.ButtonJustReleased)));
		m_SwitchCursorHint.SetLabel(UIStrings.Instance.HUDTexts.Pointer);
		AddDisposable(m_OpenMapHint.Bind(inputLayer.AddButton(OpenMap, 17, InputActionEventType.ButtonJustLongPressed)));
		m_OpenMapHint.SetLabel(UIStrings.Instance.MainMenu.LocalMap);
		AddDisposable(m_SwitchHighlightHint.Bind(inputLayer.AddButton(SwitchHighlight, 17, InputActionEventType.ButtonJustReleased)));
		m_SwitchHighlightHint.SetLabel(UIStrings.Instance.HUDTexts.HighlightObjects);
		AddDisposable(m_PauseGameHint.Bind(inputLayer.AddButton(PauseGame, 10, InputActionEventType.ButtonJustReleased)));
		m_PauseGameHint.SetLabel(UIStrings.Instance.HUDTexts.Pause);
		AddDisposable(m_CoopRolesHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OpenNetRoles();
		}, 18, base.ViewModel.NetFirstLoadState, InputActionEventType.ButtonJustLongPressed)));
		m_CoopRolesHint.SetLabel(UIStrings.Instance.EscapeMenu.EscMenuRoles);
		AddDisposable(m_EscMenuHint.Bind(inputLayer.AddButton(delegate
		{
		}, 16, InputActionEventType.ButtonJustReleased)));
		m_EscMenuHint.SetLabel(UIStrings.Instance.MainMenu.Settings);
		m_ActionBarConsoleView.AddInput(inputLayer, inCombat: false);
		m_CombatLogConsoleView.AddInput(inputLayer);
	}

	public void AddCombatInput(InputLayer inputLayer)
	{
		AddDisposable(m_FocusOnCurrentUnitCombatHint.Bind(inputLayer.AddButton(FocusOnCurrentUnit, 19, base.ViewModel.DeploymentPhase.Not().ToReadOnlyReactiveProperty(initialValue: false), InputActionEventType.ButtonJustReleased)));
		m_FocusOnCurrentUnitCombatHint.SetLabel(UIStrings.Instance.HUDTexts.FocusOnCurrentUnit);
		AddDisposable(m_HighlightObjectsCombatHint.Bind(inputLayer.AddButton(delegate
		{
		}, 12)));
		m_HighlightObjectsCombatHint.SetLabel(UIStrings.Instance.HUDTexts.HighlightObjects);
		AddDisposable(m_EndTurnCombatHint.Bind(inputLayer.AddButton(EndTurn, 17, base.ViewModel.DeploymentPhase.Not().And(base.ViewModel.IsTurnBasedActive).And(base.ViewModel.ShowEndTurn)
			.And(base.ViewModel.CanEndTurn)
			.ToReadOnlyReactiveProperty(initialValue: false))));
		m_EndTurnCombatHint.SetLabel(UIStrings.Instance.HUDTexts.EndTurn);
		AddDisposable(m_StartBattleHint.Bind(inputLayer.AddButton(EndTurn, 17, base.ViewModel.DeploymentPhase)));
		m_StartBattleHint.SetLabel(UIStrings.Instance.TurnBasedTexts.StartBattle);
		AddDisposable(m_CoopRolesSurfaceCombatHint.Bind(inputLayer.AddButton(delegate
		{
			base.ViewModel.OpenNetRoles();
		}, 18, base.ViewModel.NetFirstLoadState, InputActionEventType.ButtonJustLongPressed)));
		m_CoopRolesSurfaceCombatHint.SetLabel(UIStrings.Instance.EscapeMenu.EscMenuRoles);
		AddDisposable(m_EscMenuCombatHint.Bind(inputLayer.AddButton(delegate
		{
		}, 16, InputActionEventType.ButtonJustReleased)));
		m_EscMenuCombatHint.SetLabel(UIStrings.Instance.MainMenu.Settings);
		AddDisposable(inputLayer.AddButton(delegate
		{
			SpeedUp(state: true);
		}, 17));
		AddDisposable(inputLayer.AddButton(delegate
		{
			SpeedUp(state: false);
		}, 17, InputActionEventType.ButtonJustReleased));
		AddDisposable(inputLayer.AddButton(delegate
		{
			SpeedUp(state: false);
		}, 17, InputActionEventType.ButtonLongPressJustReleased));
		m_ActionBarConsoleView.AddInput(inputLayer, inCombat: true);
		m_CombatLogConsoleView.AddInputToCombat(inputLayer);
		m_PartyConsoleView.AddInput(inputLayer, base.ViewModel.DeploymentPhase);
	}

	public void OnShowEscMenu(InputActionEventData data)
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_IsPaused.Value = Game.Instance.IsPaused;
		if (CutsceneLock.Active && m_CutSceneInputLayer == null && gameMode == GameModeType.Cutscene)
		{
			m_CutSceneInputLayer = new InputLayer
			{
				ContextName = "CutSceneInputLayer"
			};
			AddDisposable(m_CutSceneInputLayer.AddButton(OnShowEscMenu, 16, InputActionEventType.ButtonJustReleased));
			AddDisposable(m_SkipCutsceneHintStruct = m_CutSceneInputLayer.AddButton(OnCutSceneDecline, 9, m_IsSkipCutsceneHintActive, InputActionEventType.ButtonJustLongPressed));
			AddDisposable(m_SkipCutsceneHint.Bind(m_SkipCutsceneHintStruct));
			AddDisposable(m_IsSkipCutsceneHintActive.Subscribe(HandleSkipCutsceneHintState));
			m_SkipCutsceneHint.SetLabel(UIStrings.Instance.CommonTexts.SkipHold);
			AddDisposable(GamePad.Instance.PushLayer(m_CutSceneInputLayer));
		}
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.Dialog)
		{
			InteractionHighlightController.Instance?.Highlight(on: false);
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
		m_IsPaused.Value = Game.Instance.IsPaused;
		if (!CutsceneLock.Active && m_CutSceneInputLayer != null)
		{
			GamePad.Instance.PopLayer(m_CutSceneInputLayer);
			m_CutSceneInputLayer = null;
			if (m_HideCloseCutsceneHint != null)
			{
				HideCutsceneHints();
				StopCoroutine(m_HideCloseCutsceneHint);
			}
		}
	}

	public void CloseCutscene()
	{
		if (m_CutSceneInputLayer != null)
		{
			GamePad.Instance.PopLayer(m_CutSceneInputLayer);
			m_CutSceneInputLayer = null;
			if (m_HideCloseCutsceneHint != null)
			{
				HideCutsceneHints();
				StopCoroutine(m_HideCloseCutsceneHint);
			}
		}
	}

	private void HideCutsceneHints()
	{
		m_IsSkipCutsceneHintActive.Value = false;
		m_IsSkipCutsceneLongPressAnimActive.Value = false;
		m_SkipCutsceneHintHolderFade.DisappearAnimation();
	}

	private void HandleSkipCutsceneHintState(bool value)
	{
		if (!Game.Instance.EntityPools.Cutscenes.TryFind((CutscenePlayerData p) => p.HasActiveLockControl && p.Cutscene.NonSkippable, out var _))
		{
			m_SkipCutsceneHintHolderFade.AppearAnimation();
			if (m_HideCloseCutsceneHint != null)
			{
				StopCoroutine(m_HideCloseCutsceneHint);
			}
			m_HideCloseCutsceneHint = HandleSkipHint();
			StartCoroutine(m_HideCloseCutsceneHint);
		}
	}

	private IEnumerator HandleSkipHint()
	{
		yield return new WaitForSecondsRealtime(3f);
		m_IsSkipCutsceneHintActive.Value = false;
		m_IsSkipCutsceneLongPressAnimActive.Value = false;
		m_SkipCutsceneHintHolderFade.DisappearAnimation();
	}

	private void OnCutSceneDecline(InputActionEventData data)
	{
		if (!(Game.Instance.CurrentModeType != GameModeType.Cutscene))
		{
			EventBus.RaiseEvent(delegate(ISubtitleBarkHandler h)
			{
				h.HandleOnHideBark();
			});
			m_IsSkipCutsceneHintActive.Value = false;
			m_IsSkipCutsceneLongPressAnimActive.Value = false;
			m_SkipCutsceneHintHolderFade.DisappearAnimation();
			Game.Instance.GameCommandQueue.SkipCutscene();
		}
	}

	public void SwitchPartySelector(bool isEnabled)
	{
		bool flag = RootUIContext.Instance.IsBlockedFullScreenUIType();
		FullScreenUIType fullScreenUIType = RootUIContext.Instance.FullScreenUIType;
		bool flag2 = fullScreenUIType == FullScreenUIType.Encyclopedia || fullScreenUIType == FullScreenUIType.Journal;
		bool flag3 = isEnabled && !flag && !flag2;
		if (!Game.Instance.Player.IsInCombat)
		{
			OpenPartySelector(flag3);
			return;
		}
		if (m_PartySelectorConsoleView.ViewModel != null)
		{
			m_PartySelectorConsoleView.Bind(null);
		}
		if (RootUIContext.Instance.CurrentServiceWindow != 0)
		{
			OpenPartySelector(flag3);
		}
		else
		{
			InteractionHighlightController.Instance.Highlight(flag3);
		}
	}

	private void OpenPartySelector(bool isEnableToOpen)
	{
		m_PartySelectorConsoleView.Bind(isEnableToOpen ? base.ViewModel.PartyVM : null);
		m_PartyConsoleView.PartySelectorEnabled = isEnableToOpen;
		if (isEnableToOpen)
		{
			TooltipHelper.HideTooltip();
		}
	}

	public void SwitchIngameMenu(bool isEnabled)
	{
		bool flag = RootUIContext.Instance.IsBlockedFullScreenUIType() || m_PartySelectorConsoleView.ViewModel != null || RootUIContext.Instance.IsVendorSelectingWindowShow;
		bool flag2 = isEnabled && !flag && !LoadingProcess.Instance.IsLoadingScreenActive;
		m_IngameMenuConsoleView.Bind(flag2 ? base.ViewModel.IngameMenuVM : null);
		if (isEnabled && !flag)
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void PauseGame(InputActionEventData eventData)
	{
		Game.Instance.PauseBind();
	}

	private void SwitchHighlight(InputActionEventData eventData)
	{
		InteractionHighlightController.Instance.SwitchHighlight();
	}

	private void OpenMap(InputActionEventData eventData)
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenLocalMap();
		});
	}

	private void EndTurn(InputActionEventData eventData)
	{
		Game.Instance.EndTurnBind();
	}

	private void SpeedUp(bool state)
	{
		Game.Instance.SpeedUp(state);
	}

	private void ChangeCameraRotateMode(InputActionEventData data)
	{
		UISounds.Instance.Play(UISounds.Instance.Sounds.Buttons.ButtonClick);
		Game.Instance.Player.IsCameraRotateMode = !Game.Instance.Player.IsCameraRotateMode;
	}

	private void SwitchCursor(InputLayer inputLayer)
	{
		SurfaceMainInputLayer obj = (SurfaceMainInputLayer)inputLayer;
		obj.StopMovement();
		obj.SwitchCursorEnabled();
	}

	private void FocusOnCurrentUnit(InputActionEventData data)
	{
		Game.Instance.Controllers.CameraController?.Follower?.ScrollTo((base.ViewModel.CurrentUnit.CurrentValue != null) ? base.ViewModel.CurrentUnit.CurrentValue.MechanicEntity : Game.Instance.Player.MainCharacterEntity);
	}

	protected override void DestroyViewImplementation()
	{
		m_SelectedUnitSubscription?.Dispose();
		m_SelectedUnitSubscription = null;
		m_TurnUnitSubscription?.Dispose();
		m_TurnUnitSubscription = null;
		if (m_CutSceneInputLayer != null)
		{
			GamePad.Instance.PopLayer(m_CutSceneInputLayer);
			m_CutSceneInputLayer = null;
		}
		if (m_HideCloseCutsceneHint != null)
		{
			HideCutsceneHints();
			StopCoroutine(m_HideCloseCutsceneHint);
			m_HideCloseCutsceneHint = null;
		}
	}
}
