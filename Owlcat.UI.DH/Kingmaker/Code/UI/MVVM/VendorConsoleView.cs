using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorConsoleView : VendorView<InventoryStashConsoleView, ItemsFilterConsoleView, VendorLevelItemsConsoleView, VendorTransitionWindowConsoleView>
{
	[Header("TooltipPlaces")]
	[SerializeField]
	private TooltipPlaces m_StashTooltipPlaces;

	[SerializeField]
	private TooltipPlaces m_CenterTooltipPlaces;

	[Header("Console")]
	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private ConsoleHint m_NextWindowHint;

	[SerializeField]
	private ConsoleHint m_PrevWindowHint;

	private InputLayer m_InputLayer;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private ReactiveProperty<bool> IsVendorSelected = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> IsPlayerStashSelected = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> IsReputationSelected = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_ShowTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasTooltip = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> m_CanSendToCargo = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> m_CanSendToInventory = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> m_IsVendorTradeItem = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> m_IsVendorBuyItem = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> CanSell = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> HasCargo = new ReactiveProperty<bool>();

	private List<IDisposable> m_DisposableBinds = new List<IDisposable>();

	private List<IDisposable> m_DisposableVendorBinds = new List<IDisposable>();

	private List<IDisposable> m_Disposables = new List<IDisposable>();

	private TooltipConfig m_MainTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private TooltipConfig m_CompareTooltipConfig = new TooltipConfig
	{
		InfoCallConsoleMethod = InfoCallConsoleMethod.None
	};

	private IDisposable m_UpdateBindsDelay;

	protected override void OnBind()
	{
		base.OnBind();
		DisposeReputationBind();
		DisposeVendorBind();
		CreateNavigation();
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnEntityFocused).AddTo(this);
		base.ViewModel.ActiveTab.Subscribe(delegate
		{
			UpdateNavigation();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_VendorTradePartView.OnUpdateSlots, delegate
		{
			UpdateNavigation();
		}).AddTo(this);
		base.ViewModel.ActiveTab.Subscribe(delegate
		{
			UpdateBindsDelayed();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Disposables.ForEach(delegate(IDisposable d)
		{
			d.Dispose();
		});
		m_Disposables.Clear();
		DisposeVendorBind();
		DisposeReputationBind();
		TooltipHelper.HideTooltip();
		base.OnUnbind();
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour();
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "Vendor Console View"
		});
		VendorTradePartConsoleView vendorTradePartConsoleView = m_VendorTradePartView as VendorTradePartConsoleView;
		if (vendorTradePartConsoleView != null)
		{
			m_NavigationBehaviour.AddEntityGrid(vendorTradePartConsoleView.GetNavigation());
		}
		m_NavigationBehaviour.AddEntityGrid(m_StashView.GetNavigation());
		m_NavigationBehaviour.FocusOnFirstValidEntity();
		GamePad.Instance.PushLayer(m_InputLayer).AddTo(this);
	}

	private void UpdateBindsDelayed()
	{
		m_UpdateBindsDelay?.Dispose();
		m_UpdateBindsDelay = Observable.TimerFrame(3).Subscribe(UpdateBinds).AddTo(this);
	}

	private void UpdateNavigation()
	{
		m_NavigationBehaviour.Clear();
		if (base.ViewModel.ActiveTab.CurrentValue == VendorWindowsTab.Trade)
		{
			VendorTradePartConsoleView vendorTradePartConsoleView = m_VendorTradePartView as VendorTradePartConsoleView;
			if (vendorTradePartConsoleView != null)
			{
				m_NavigationBehaviour.AddEntityGrid(vendorTradePartConsoleView.GetNavigation());
			}
			m_NavigationBehaviour.AddEntityGrid(m_StashView.GetNavigation());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
		}
		else
		{
			m_NavigationBehaviour.AddEntityGrid(m_StashView.GetNavigation());
			m_NavigationBehaviour.FocusOnFirstValidEntity();
		}
	}

	private void UpdateBinds()
	{
		m_InputLayer.Unbind();
		if (base.ViewModel.ActiveTab.CurrentValue == VendorWindowsTab.Trade)
		{
			CreateInput();
		}
		m_InputLayer.Bind();
	}

	private void DisposeReputationBind()
	{
		m_DisposableBinds.ForEach(delegate(IDisposable d)
		{
			d?.Dispose();
		});
		m_DisposableBinds.Clear();
	}

	private void DisposeVendorBind()
	{
		m_DisposableVendorBinds.ForEach(delegate(IDisposable d)
		{
			d?.Dispose();
		});
		m_DisposableVendorBinds.Clear();
	}

	private void Refocus()
	{
		m_NavigationBehaviour.FocusOnFirstValidEntity();
	}

	private void NeededRefocus()
	{
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip.Value = !m_ShowTooltip.Value;
		OnEntityFocused(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void HandleTooltip(IConsoleEntity entity)
	{
		TooltipHelper.HideTooltip();
		UpdateTooltipConfigs();
		if (entity == null)
		{
			m_HasTooltip.Value = false;
			return;
		}
		MonoBehaviour monoBehaviour = (entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour;
		if (monoBehaviour == null)
		{
			m_HasTooltip.Value = false;
		}
		else if (entity is SimpleConsoleNavigationEntity simpleConsoleNavigationEntity)
		{
			m_HasTooltip.Value = simpleConsoleNavigationEntity.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				MonoBehaviour tooltipPlace = simpleConsoleNavigationEntity.GetTooltipPlace();
				if ((bool)tooltipPlace)
				{
					tooltipPlace.ShowConsoleTooltip(simpleConsoleNavigationEntity.TooltipTemplate(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
				}
				else
				{
					monoBehaviour.ShowConsoleTooltip(simpleConsoleNavigationEntity.TooltipTemplate(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
				}
			}
		}
		else if (entity is IHasTooltipTemplate hasTooltipTemplate)
		{
			m_HasTooltip.Value = hasTooltipTemplate.TooltipTemplate() != null;
			if (m_ShowTooltip.Value)
			{
				monoBehaviour.ShowConsoleTooltip(hasTooltipTemplate.TooltipTemplate(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
			}
		}
		else if (entity is IHasTooltipTemplates hasTooltipTemplates)
		{
			List<TooltipBaseTemplate> list = hasTooltipTemplates.TooltipTemplates();
			m_HasTooltip.Value = list.Count > 0;
			if (m_HasTooltip.Value && m_ShowTooltip.Value)
			{
				if (list.Count > 1)
				{
					m_CompareTooltipConfig.MaxHeight = ((list.Count > 2) ? 450 : 0);
					monoBehaviour.ShowComparativeTooltip(hasTooltipTemplates.TooltipTemplates(), m_MainTooltipConfig, m_CompareTooltipConfig, showScrollbar: true);
				}
				else
				{
					monoBehaviour.ShowConsoleTooltip(list.LastOrDefault(), m_NavigationBehaviour, m_MainTooltipConfig, shouldNotHideLittleTooltip: false, showScrollbar: true);
				}
			}
		}
		else
		{
			m_HasTooltip.Value = false;
		}
	}

	private void UpdateTooltipConfigs()
	{
		TooltipPlaces tooltipPlaces = (IsPlayerStashSelected.Value ? m_StashTooltipPlaces : m_CenterTooltipPlaces);
		m_MainTooltipConfig = tooltipPlaces.GetMainTooltipConfig(m_MainTooltipConfig);
		m_CompareTooltipConfig = tooltipPlaces.GetCompareTooltipConfig(m_CompareTooltipConfig);
	}

	private void CreateInput()
	{
		DisposeVendorBind();
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(delegate
		{
			OnDeclineClick();
		}, 9);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.CloseWindow));
		m_DisposableVendorBinds.Add(inputBindStruct);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(delegate
		{
			SetNextTab();
		}, 14, IsPlayerStashSelected.Not().ToReadOnlyReactiveProperty(initialValue: false));
		m_DisposableVendorBinds.Add(m_PrevWindowHint.Bind(inputBindStruct2));
		m_DisposableVendorBinds.Add(inputBindStruct2);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(delegate
		{
			SetNextTab();
		}, 15, IsPlayerStashSelected.Not().ToReadOnlyReactiveProperty(initialValue: false));
		m_DisposableVendorBinds.Add(m_NextWindowHint.Bind(inputBindStruct3));
		m_DisposableVendorBinds.Add(inputBindStruct3);
		InputBindStruct inputBindStruct4 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_IsVendorBuyItem, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.ContextMenu.Buy));
		m_DisposableVendorBinds.Add(inputBindStruct4);
		InputBindStruct item = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanSendToCargo, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(item);
		InputBindStruct inputBindStruct5 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_CanSendToInventory, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.LootWindow.SendToInventory));
		m_DisposableVendorBinds.Add(inputBindStruct5);
		InputBindStruct inputBindStruct6 = m_InputLayer.AddButton(delegate
		{
		}, 8, m_IsVendorTradeItem, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct6, UIStrings.Instance.CommonTexts.Select));
		m_DisposableVendorBinds.Add(inputBindStruct6);
		InputBindStruct inputBindStruct7 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		m_DisposableVendorBinds.Add(m_HintsWidget.BindHint(inputBindStruct7, UIStrings.Instance.CommonTexts.Information, ConsoleHintsWidget.HintPosition.Left));
		m_DisposableVendorBinds.Add(inputBindStruct7);
		m_DisposableVendorBinds.AddRange(m_StashView.ItemsFilter.AddInputDisposable(m_InputLayer, IsPlayerStashSelected));
	}

	private void OnEntityFocused(IConsoleEntity currentFocus)
	{
		InventorySlotConsoleView inventorySlotConsoleView = currentFocus as InventorySlotConsoleView;
		bool flag = (bool)inventorySlotConsoleView && inventorySlotConsoleView.Item != null;
		m_IsVendorTradeItem.Value = true;
		m_CanSendToCargo.Value = (bool)inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.IsInStash && flag;
		m_CanSendToInventory.Value = flag && !inventorySlotConsoleView.SlotVM.IsTrash.CurrentValue;
		m_IsVendorBuyItem.Value = currentFocus is VendorSlotConsoleView vendorSlotConsoleView && vendorSlotConsoleView.CanConfirmClick();
		SetupNewFilterSelected(currentFocus);
		ScrollToObject(currentFocus);
		HandleTooltip(currentFocus);
	}

	private void SetupNewFilterSelected(IConsoleEntity currentFocus)
	{
		InventorySlotConsoleView inventorySlotConsoleView = currentFocus as InventorySlotConsoleView;
		if ((bool)inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.SlotsGroupType == ItemSlotsGroupType.Inventory && m_VendorTabNavigation.CurrentTab.CurrentValue == VendorWindowsTab.Trade)
		{
			IsVendorSelected.Value = false;
			IsReputationSelected.Value = false;
			IsPlayerStashSelected.Value = true;
		}
		else if ((bool)inventorySlotConsoleView && inventorySlotConsoleView.SlotVM.SlotsGroupType == ItemSlotsGroupType.Inventory && m_VendorTabNavigation.CurrentTab.CurrentValue == VendorWindowsTab.Reputation)
		{
			IsVendorSelected.Value = false;
			IsReputationSelected.Value = false;
			IsPlayerStashSelected.Value = true;
		}
		else if (m_VendorTabNavigation.CurrentTab.CurrentValue == VendorWindowsTab.Trade)
		{
			IsVendorSelected.Value = true;
			IsReputationSelected.Value = false;
			IsPlayerStashSelected.Value = false;
		}
		else if (m_VendorTabNavigation.CurrentTab.CurrentValue == VendorWindowsTab.Reputation)
		{
			IsVendorSelected.Value = false;
			IsReputationSelected.Value = true;
			IsPlayerStashSelected.Value = false;
		}
	}

	private void ScrollToObject(IConsoleEntity entity)
	{
		_ = entity is VendorSlotConsoleView;
	}

	private void SetNextTab()
	{
		m_SelectorView.SetNextTab();
		m_VendorTabNavigation.SetNextTab();
	}

	private void OnDeclineClick()
	{
		TooltipHelper.HideTooltip();
		if (m_HasTooltip.Value && m_ShowTooltip.Value)
		{
			m_ShowTooltip.Value = false;
		}
		else
		{
			base.ViewModel.Close();
		}
	}
}
