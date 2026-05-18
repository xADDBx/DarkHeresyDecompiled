using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
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
	private readonly TooltipsDataCache m_TooltipsDataCache;

	private readonly ReactiveProperty<TooltipVM> m_TooltipVM;

	private readonly ReactiveProperty<HintVM> m_HintVM;

	private readonly ReactiveProperty<InfoWindowVM> m_InfoWindowVM;

	private readonly ReactiveProperty<InfoWindowVM> m_GlossaryInfoWindowVM;

	private readonly ReactiveProperty<ComparativeTooltipVM> m_ComparativeTooltipVM;

	private IDisposable m_DelayedShowTooltipHandle;

	private IDisposable m_DelayedShowHintHandle;

	private Vector2? m_LastTooltipPosition;

	private bool m_MustHide;

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
		m_TooltipsDataCache = new TooltipsDataCache().AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(TooltipVM.Select((TooltipVM t) => (t != null) ? Observable.Never<Unit>() : Observable.Timer(TimeSpan.FromMilliseconds(200.0))).Switch(), delegate
		{
			m_LastTooltipPosition = null;
		}).AddTo(this);
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

	public void HandleInfoRequest(TooltipBaseTemplate template, bool shouldNotHideLittleTooltip = false)
	{
		Vector2? lastTooltipPosition = ((InfoWindowVM.CurrentValue == null) ? m_LastTooltipPosition : null);
		DisposeInfoWindow();
		if (template != null)
		{
			m_InfoWindowVM.Value = new InfoWindowVM(template, delegate
			{
				DisposeInfoWindow();
			}, shouldNotHideLittleTooltip, lastTooltipPosition).AddTo(this);
		}
	}

	public void HandleMultipleInfoRequest(IEnumerable<TooltipBaseTemplate> templates)
	{
		DisposeInfoWindow();
		if (templates != null)
		{
			m_InfoWindowVM.Value = new InfoWindowVM(templates, delegate
			{
				DisposeInfoWindow();
			}).AddTo(this);
		}
	}

	public void HandleGlossaryInfoRequest(TooltipTemplateGlossary template)
	{
		if (template != null)
		{
			TooltipHelper.AddGlossaryHistory(template.GlossaryEntry);
			m_GlossaryInfoWindowVM.Value = new InfoWindowVM(template, DisposeGlossaryInfoWindow).AddTo(this);
		}
		else
		{
			DisposeGlossaryInfoWindow();
		}
	}

	public void HandleComparativeTooltipRequest(Transform source, IEnumerable<TooltipData> data, bool showScrollbar = false)
	{
		DisposeComparativeTooltip();
		List<TooltipData> list = data?.ToList() ?? new List<TooltipData>();
		if (list.Count > 0)
		{
			List<TooltipData> list2 = new List<TooltipData>();
			if (list.Count > 1)
			{
				List<TooltipData> list3 = list2;
				List<TooltipData> list4 = list;
				list3.Add(list4[list4.Count - 1]);
				list.RemoveLast();
				List<TooltipData> list5 = list2;
				List<TooltipData> list6 = list;
				list = list5;
				list2 = list6;
			}
			m_ComparativeTooltipVM.Value = new ComparativeTooltipVM(source, list, list2, showScrollbar).AddTo(this);
			ContextMenuHelper.HideContextMenu();
		}
	}

	public void HandleComparativeTooltipRequest(Transform source, IEnumerable<TooltipData> mainData, IEnumerable<TooltipData> compareData, bool showScrollbar = false)
	{
		DisposeComparativeTooltip();
		List<TooltipData> list = mainData?.ToList() ?? new List<TooltipData>();
		List<TooltipData> list2 = compareData?.ToList() ?? new List<TooltipData>();
		if (list.Count > 0 || list2.Count > 0)
		{
			m_ComparativeTooltipVM.Value = new ComparativeTooltipVM(source, list, list2, showScrollbar).AddTo(this);
			ContextMenuHelper.HideContextMenu();
		}
	}

	public void HandleHintRequest(HintData data, bool shouldShow)
	{
		if (data != null && shouldShow)
		{
			m_MustHide = false;
			TimeSpan dueTime = TimeSpan.FromSeconds((float)SettingsRoot.Game.Tooltips.ShowDelay);
			m_DelayedShowHintHandle = ObservableSubscribeExtensions.Subscribe(Observable.Timer(dueTime, UnityTimeProvider.UpdateIgnoreTimeScale), delegate
			{
				if (!m_MustHide)
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
		m_MustHide = true;
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
		if (gameMode == GameModeType.Cutscene || gameMode == GameModeType.GameOver || gameMode == GameModeType.CutsceneGlobalMap || gameMode == GameModeType.Dialog)
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

	private void DisposeAll()
	{
		m_TooltipsDataCache.Clear();
		DisposeTooltip();
		DisposeComparativeTooltip();
		DisposeHint();
		DisposeInfoWindow();
		DisposeGlossaryInfoWindow();
	}
}
