using System;
using System.Collections;
using Kingmaker.AreaLogic.Cutscenes;
using Kingmaker.Code.UI.MVVM.View;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.MapObjects;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.GameCommands;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Sound;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
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
	private HintView m_SkipCutsceneHint;

	[SerializeField]
	private FadeAnimator m_SkipCutsceneHintHolderFade;

	private IDisposable m_TurnUnitSubscription;

	private IDisposable m_SelectedUnitSubscription;

	private readonly ReactiveProperty<bool> m_IsSkipCutsceneHintActive = new ReactiveProperty<bool>(value: false);

	private readonly ReactiveProperty<bool> m_IsSkipCutsceneLongPressAnimActive = new ReactiveProperty<bool>();

	private IEnumerator m_HideCloseCutsceneHint;

	[Header("Main input hints")]
	[SerializeField]
	private HintView m_PauseGameHint;

	[SerializeField]
	private FadeAnimator m_AdditionalHintsContainer;

	private IDisposable m_DisappearTask;

	[SerializeField]
	private float m_HintsDisappearTime = 5f;

	[SerializeField]
	private HintView m_ChangeCameraRotateModeHint;

	[SerializeField]
	private HintView m_FocusOnCurrentUnitHint;

	[SerializeField]
	private HintView m_SwitchCursorHint;

	[SerializeField]
	private HintView m_OpenMapHint;

	[SerializeField]
	private HintView m_SwitchHighlightHint;

	[SerializeField]
	private HintView m_CoopRolesHint;

	[SerializeField]
	private HintView m_EscMenuHint;

	[Header("Combat input hints")]
	[SerializeField]
	private HintView m_FocusOnCurrentUnitCombatHint;

	[SerializeField]
	private HintView m_HighlightObjectsCombatHint;

	[SerializeField]
	private HintView m_EndTurnCombatHint;

	[SerializeField]
	private HintView m_StartBattleHint;

	[SerializeField]
	private HintView m_CoopRolesSurfaceCombatHint;

	[SerializeField]
	private HintView m_EscMenuCombatHint;

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
		AddDisposable(base.ViewModel.PreparationTurnWindowVM.Subscribe(m_CombatStartWindowView.Bind));
		AddDisposable(base.ViewModel.CurrentUnit.Subscribe(m_SurfaceCombatCurrentUnitView.Bind));
		AddDisposable(base.ViewModel.ActionBarVM.IsVisible.Subscribe(ActionBarVisibilityChanged));
		AddDisposable(ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
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

	public void AddBaseInput()
	{
	}

	public void AddMainInput()
	{
	}

	public void AddCombatInput()
	{
	}

	public void OnShowEscMenu()
	{
		EventBus.RaiseEvent(delegate(IEscMenuHandler h)
		{
			h.HandleOpen();
		});
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		m_IsPaused.Value = Game.Instance.IsPaused;
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void CloseCutscene()
	{
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

	private void OnCutSceneDecline()
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
		bool flag = RootUIContext.Instance.IsBlockedFullScreenUIType() || m_PartySelectorConsoleView.ViewModel != null;
		bool flag2 = isEnabled && !flag && !LoadingProcess.Instance.IsLoadingScreenActive;
		m_IngameMenuConsoleView.Bind(flag2 ? base.ViewModel.IngameMenuVM : null);
		if (isEnabled && !flag)
		{
			TooltipHelper.HideTooltip();
		}
	}

	private void PauseGame()
	{
		Game.Instance.PauseBind();
	}

	private void SwitchHighlight()
	{
		InteractionHighlightController.Instance.SwitchHighlight();
	}

	private void OpenMap()
	{
		EventBus.RaiseEvent(delegate(IServiceWindowUIHandler h)
		{
			h.HandleOpenLocalMap();
		});
	}

	private void EndTurn()
	{
		Game.Instance.EndTurnBind();
	}

	private void SpeedUp(bool state)
	{
		Game.Instance.SpeedUp(state);
	}

	private void ChangeCameraRotateMode()
	{
		ButtonsSounds.Instance.Default.Click.Play();
		Game.Instance.Player.IsCameraRotateMode = !Game.Instance.Player.IsCameraRotateMode;
	}

	private void SwitchCursor()
	{
	}

	private void FocusOnCurrentUnit()
	{
		Game.Instance.Controllers.CameraController?.Follower?.ScrollTo((base.ViewModel.CurrentUnit.CurrentValue != null) ? base.ViewModel.CurrentUnit.CurrentValue.MechanicEntity : Game.Instance.Player.MainCharacterEntity);
	}

	protected override void DestroyViewImplementation()
	{
		m_SelectedUnitSubscription?.Dispose();
		m_SelectedUnitSubscription = null;
		m_TurnUnitSubscription?.Dispose();
		m_TurnUnitSubscription = null;
		if (m_HideCloseCutsceneHint != null)
		{
			HideCutsceneHints();
			StopCoroutine(m_HideCloseCutsceneHint);
			m_HideCloseCutsceneHint = null;
		}
	}
}
