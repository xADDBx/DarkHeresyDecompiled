using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Controllers.Dialog;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.GameModes;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipContext : ViewModel, ITooltipBaseHandler, ISubscriber, ITooltipHandler, ITurnStartHandler, ISubscriber<IMechanicEntity>, IInteractionHighlightUIHandler, IGameModeHandler, ITurnBasedModeHandler, ITurnBasedModeStartHandler, IAreaHandler, IAdditiveAreaSwitchHandler, ILootInteractionHandler, ISubscriber<IBaseUnitEntity>, ITradeStateChanged
{
	private readonly ReactiveProperty<TooltipVM> m_TooltipVM = new ReactiveProperty<TooltipVM>();

	private readonly ReactiveProperty<HintVM> m_HintVM = new ReactiveProperty<HintVM>();

	private readonly ReactiveProperty<InfoWindowVM> m_InfoWindowVM = new ReactiveProperty<InfoWindowVM>();

	private readonly ReactiveProperty<InfoWindowVM> m_GlossaryInfoWindowVM = new ReactiveProperty<InfoWindowVM>();

	private readonly ReactiveProperty<ComparativeTooltipVM> m_ComparativeTooltipVM = new ReactiveProperty<ComparativeTooltipVM>();

	public readonly TooltipsDataCache TooltipsDataCache;

	private IDisposable m_DelayedShowTooltipHandle;

	private IDisposable m_DelayedShowHintHandle;

	private Vector2? m_LastTooltipPosition;

	private bool m_MussHide;

	public ReadOnlyReactiveProperty<TooltipVM> TooltipVM => m_TooltipVM;

	public ReadOnlyReactiveProperty<HintVM> HintVM => m_HintVM;

	public ReadOnlyReactiveProperty<InfoWindowVM> InfoWindowVM => m_InfoWindowVM;

	public ReadOnlyReactiveProperty<InfoWindowVM> GlossaryInfoWindowVM => m_GlossaryInfoWindowVM;

	public ReadOnlyReactiveProperty<ComparativeTooltipVM> ComparativeTooltipVM => m_ComparativeTooltipVM;

	public TooltipContext(ReactiveProperty<TooltipVM> tooltipVM, ReactiveProperty<HintVM> hintVM, ReactiveProperty<InfoWindowVM> infoWindowVM, ReactiveProperty<InfoWindowVM> glossaryInfoWindowVM, ReactiveProperty<ComparativeTooltipVM> comparativeTooltipVM)
	{
		m_TooltipVM = tooltipVM;
		m_HintVM = hintVM;
		m_InfoWindowVM = infoWindowVM;
		m_GlossaryInfoWindowVM = glossaryInfoWindowVM;
		m_ComparativeTooltipVM = comparativeTooltipVM;
		TooltipsDataCache = new TooltipsDataCache().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		TooltipVM.Where((TooltipVM t) => t == null).Timeout(TimeSpan.FromMilliseconds(200.0)).Subscribe(delegate
		{
			m_LastTooltipPosition = null;
		})
			.AddTo(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		DisposeAll();
	}

	public void HandleTooltipRequest(TooltipData data, bool shouldNotHideLittleTooltip = false, bool showScrollbar = false)
	{
		DisposeTooltip();
		if (data?.MainTemplate != null)
		{
			Game.Instance.CursorController.SetCursor(CursorType.Info, force: true);
			m_DelayedShowTooltipHandle = DelayedInvoker.InvokeInTime(delegate
			{
				m_TooltipVM.Value = new TooltipVM(data, isComparative: false, shouldNotHideLittleTooltip, showScrollbar).AddTo(this);
			}, SettingsRoot.Game.Tooltips.ShowDelay);
		}
	}

	public void HandleInfoRequest(TooltipBaseTemplate template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null, bool shouldNotHideLittleTooltip = false)
	{
		Vector2? lastTooltipPosition = ((InfoWindowVM.CurrentValue == null) ? m_LastTooltipPosition : null);
		DisposeInfoWindow();
		if (template != null)
		{
			ownerNavigationBehaviour?.UnFocusCurrentEntity();
			m_InfoWindowVM.Value = new InfoWindowVM(template, delegate
			{
				DisposeInfoWindow();
				ownerNavigationBehaviour?.FocusOnCurrentEntity();
			}, shouldNotHideLittleTooltip, lastTooltipPosition).AddTo(this);
		}
	}

	public void HandleMultipleInfoRequest(IEnumerable<TooltipBaseTemplate> templates, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
		DisposeInfoWindow();
		if (templates != null)
		{
			ownerNavigationBehaviour?.UnFocusCurrentEntity();
			m_InfoWindowVM.Value = new InfoWindowVM(templates, delegate
			{
				DisposeInfoWindow();
				ownerNavigationBehaviour?.FocusOnCurrentEntity();
			}).AddTo(this);
		}
	}

	public void HandleGlossaryInfoRequest(TooltipTemplateGlossary template, ConsoleNavigationBehaviour ownerNavigationBehaviour = null)
	{
		if (template != null)
		{
			ownerNavigationBehaviour?.UnFocusCurrentEntity();
			TooltipHelper.AddGlossaryHistory(template.GlossaryEntry);
			m_GlossaryInfoWindowVM.Value = new InfoWindowVM(template, delegate
			{
				DisposeGlossaryInfoWindow();
				ownerNavigationBehaviour?.FocusOnCurrentEntity();
			}).AddTo(this);
		}
		else
		{
			DisposeGlossaryInfoWindow();
		}
	}

	public void HandleComparativeTooltipRequest(IEnumerable<TooltipData> data, bool showScrollbar = false)
	{
		DisposeComparativeTooltip();
		if (!data.Empty())
		{
			m_ComparativeTooltipVM.Value = new ComparativeTooltipVM(data, showScrollbar).AddTo(this);
			ContextMenuHelper.HideContextMenu();
		}
	}

	public void HandleHintRequest(HintData data, bool shouldShow)
	{
		if (data != null && shouldShow)
		{
			m_MussHide = false;
			TimeSpan dueTime = TimeSpan.FromSeconds((float)SettingsRoot.Game.Tooltips.ShowDelay);
			m_DelayedShowHintHandle = ObservableSubscribeExtensions.Subscribe(Observable.Timer(dueTime, UnityTimeProvider.UpdateIgnoreTimeScale), delegate
			{
				if (!m_MussHide)
				{
					m_HintVM.Value = new HintVM(data).AddTo(this);
				}
			});
		}
		else
		{
			DisposeHint();
		}
	}

	public void HandleUnitStartTurn(bool isTurnBased)
	{
		if (isTurnBased)
		{
			DisposeHint();
		}
	}

	public void HandleHighlightChange(bool isOn)
	{
		if (!isOn)
		{
			DisposeHint();
		}
	}

	private void DisposeTooltip()
	{
		LootCollectorVM lootCollectorVM = RootUIContext.Instance.RootVM.LootVM.CurrentValue?.LootCollectorExitLocation ?? RootUIContext.Instance.RootVM.LootVM.CurrentValue?.LootCollectorOnLocation;
		if (lootCollectorVM == null || !RootUIContext.Instance.RootVM.LootContext.IsShown || !lootCollectorVM.IsTrashMode.CurrentValue)
		{
			Game.Instance.CursorController.ClearCursor(force: true);
		}
		m_DelayedShowTooltipHandle?.Dispose();
		m_DelayedShowTooltipHandle = null;
		if (TooltipVM.CurrentValue != null)
		{
			m_LastTooltipPosition = TooltipVM.CurrentValue.LastPosition;
		}
		TooltipVM currentValue = TooltipVM.CurrentValue;
		m_TooltipVM.Value = null;
		currentValue?.Dispose();
	}

	private void DisposeHint()
	{
		m_MussHide = true;
		m_DelayedShowHintHandle?.Dispose();
		m_DelayedShowHintHandle = null;
		HintVM.CurrentValue?.Dispose();
		m_HintVM.Value = null;
	}

	private void DisposeInfoWindow()
	{
		InfoWindowVM.CurrentValue?.Dispose();
		m_InfoWindowVM.Value = null;
	}

	private void DisposeGlossaryInfoWindow()
	{
		GlossaryInfoWindowVM.CurrentValue?.Dispose();
		m_GlossaryInfoWindowVM.Value = null;
	}

	private void DisposeComparativeTooltip()
	{
		ComparativeTooltipVM.CurrentValue?.Dispose();
		m_ComparativeTooltipVM.Value = null;
	}

	public void OnGameModeStart(GameModeType gameMode)
	{
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.CutsceneGlobalMap || gameMode == GameModeType.StarSystem || gameMode == GameModeType.Dialog)
		{
			DisposeAll();
		}
	}

	public void OnGameModeStop(GameModeType gameMode)
	{
	}

	public void OnAreaBeginUnloading()
	{
		DisposeAll();
	}

	public void OnAreaDidLoad()
	{
	}

	public void OnAdditiveAreaBeginDeactivated()
	{
		DisposeAll();
	}

	public void OnAdditiveAreaDidActivated()
	{
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (isTurnBased)
		{
			DisposeAll();
		}
	}

	void ITurnBasedModeStartHandler.HandleTurnBasedModeStarted()
	{
		DisposeAll();
	}

	public void HandleLootInteraction(EntityViewBase[] objects, LootContainerType containerType, Action closeCallback)
	{
	}

	public void HandleSpaceLootInteraction(ILootable[] objects, LootContainerType containerType, Action closeCallback, SkillCheckResult skillCheckResult = null)
	{
	}

	public void HandleZoneLootInteraction(AreaTransitionPart areaTransition)
	{
		DisposeAll();
	}

	public void HandleBeginTrading()
	{
		DisposeAll();
	}

	public void HandleEndTrading()
	{
	}

	public void HandleVendorAboutToTrading()
	{
	}

	public void DisposeAll()
	{
		TooltipsDataCache.Clear();
		DisposeTooltip();
		DisposeHint();
		DisposeInfoWindow();
		DisposeGlossaryInfoWindow();
	}
}
