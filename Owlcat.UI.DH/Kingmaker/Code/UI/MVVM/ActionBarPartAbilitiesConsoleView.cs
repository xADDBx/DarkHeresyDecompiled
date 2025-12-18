using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using Rewired;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartAbilitiesConsoleView : ActionBarPartAbilitiesBaseView, ICullFocusHandler, ISubscriber, ICharInfoAbilitiesChooseModeHandler
{
	[SerializeField]
	private ActionBarAbilitiesRowView m_Row;

	[SerializeField]
	private ActionBarSlotAbilityConsoleView m_SlotView;

	[SerializeField]
	protected PageNavigationConsole m_PageNavigation;

	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	[SerializeField]
	private ConsoleHint m_ActivateHint;

	[SerializeField]
	private ConsoleHint m_ActivateInCombatHint;

	[SerializeField]
	private ConsoleHintsWidget m_HintsWidget;

	[SerializeField]
	private VeilThicknessConsoleView m_VeilThicknessConsoleView;

	[SerializeField]
	private AbilitySelectorWindowConsoleView m_AbilitySelectorWindowConsoleView;

	private readonly ReactiveProperty<int> m_CurrentRowIndex = new ReactiveProperty<int>();

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private InputLayer m_InputLayer;

	private InputLayer m_MoveAbilityInputLayer;

	private bool m_ShowTooltip = true;

	private readonly ReactiveProperty<bool> m_HasTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasContextMenu = new ReactiveProperty<bool>();

	private ActionBarSlotAbilityConsoleView m_CurrentAbilitySlot;

	private ActionBarSlotAbilityConsoleView m_LastAbilitySlot;

	private MechanicActionBarSlot m_CurrentMechanicSlot;

	private int m_CurrentIndex;

	private IConsoleEntity m_CulledFocus;

	public GridConsoleNavigationBehaviour NavigationBehaviour => m_NavigationBehaviour;

	public override void Initialize()
	{
		m_MoveAnimator.Or(null)?.Initialize();
		m_AbilitySelectorWindowConsoleView.Or(null)?.Initialize();
	}

	protected override void OnBind()
	{
		CreateInput();
		m_HintsWidget.AddTo(this);
		m_CurrentRowIndex.Subscribe(OnIndexChanged).AddTo(this);
		base.ViewModel.UnitChanged.Subscribe(OnUnitChanged).AddTo(this);
		base.ViewModel.SlotCountChanged.Subscribe(OnUnitChanged).AddTo(this);
		base.ViewModel.IsActive.Subscribe(OnActive).AddTo(this);
		base.ViewModel.MoveAbilityMode.Subscribe(OnMoveMode).AddTo(this);
		if ((bool)m_AbilitySelectorWindowConsoleView)
		{
			base.ViewModel.AbilitySelectorWindowVM.Subscribe(m_AbilitySelectorWindowConsoleView.Bind).AddTo(this);
		}
		base.ViewModel.AbilitySelectorWindowVM.Subscribe(delegate(AbilitySelectorWindowVM value)
		{
			if (value == null && (bool)m_LastAbilitySlot)
			{
				IConsoleEntity entity = m_NavigationBehaviour.Entities.FirstOrDefault((IConsoleEntity e) => e is ActionBarSlotAbilityConsoleView actionBarSlotAbilityConsoleView && actionBarSlotAbilityConsoleView == m_CurrentAbilitySlot);
				m_NavigationBehaviour.FocusOnEntityManual(entity);
				m_LastAbilitySlot = null;
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		OnUnitChanged();
	}

	protected override void OnUnbind()
	{
		m_PageNavigation.Dispose();
		OnMoveMode(on: false);
		OnActive(active: false);
		m_ShowTooltip = true;
		m_InputLayer = null;
		m_MoveAbilityInputLayer = null;
	}

	private void OnIndexChanged(int newIndex)
	{
		if (base.ViewModel.Slots.Count != 0)
		{
			base.ViewModel.RowIndex = newIndex;
			DrawSlots();
		}
	}

	private void OnUnitChanged()
	{
		if (base.ViewModel.Slots.Count != 0)
		{
			DrawPaginator();
		}
	}

	private void DrawPaginator()
	{
		m_CurrentRowIndex.Value = base.ViewModel.RowIndex;
		m_CurrentRowIndex.ForceNotify();
		int pageCount = Mathf.CeilToInt((float)base.ViewModel.Slots.Count / (float)base.SlotsInRow);
		m_PageNavigation.Initialize(pageCount, m_CurrentRowIndex, delegate(int idx)
		{
			m_CurrentRowIndex.Value = idx;
		});
	}

	private void DrawSlots()
	{
		int num = m_NavigationBehaviour.Entities.IndexOf(m_NavigationBehaviour.Focus.Value);
		List<ActionBarSlotVM> list = new List<ActionBarSlotVM>();
		for (int i = base.ViewModel.RowIndex * base.SlotsInRow; i < (base.ViewModel.RowIndex + 1) * base.SlotsInRow; i++)
		{
			list.Add(base.ViewModel.Slots[i]);
		}
		m_Row.DrawEntries(list, m_SlotView);
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddRow(m_Row.GetConsoleEntities());
		if (num != -1)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_NavigationBehaviour.Entities.ElementAt(num));
		}
		else if (base.ViewModel.IsActive.CurrentValue)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_Row.GetFirstValidEntity());
		}
	}

	public ActionBarSlotAbilityConsoleView GetFirstEmptySlot()
	{
		IEnumerable<ActionBarSlotAbilityConsoleView> enumerable = m_Row.GetSlots().Cast<ActionBarSlotAbilityConsoleView>();
		foreach (ActionBarSlotAbilityConsoleView item in enumerable)
		{
			if (item.IsEmpty)
			{
				return item;
			}
		}
		return enumerable.FirstOrDefault();
	}

	public void AddInput(InputLayer inputLayer, ReadOnlyReactiveProperty<bool> enable, bool inCombat)
	{
		InputBindStruct inputBindStruct = inputLayer.AddButton(delegate
		{
			Activate();
		}, 11, enable);
		if (inCombat)
		{
			m_ActivateInCombatHint.Bind(inputBindStruct).AddTo(this);
		}
		else
		{
			m_ActivateHint.Bind(inputBindStruct).AddTo(this);
		}
		inputBindStruct.AddTo(this);
	}

	public void Activate()
	{
		if (!(Game.Instance.CurrentModeType != GameModeType.Default) && (!Game.Instance.Controllers.TurnController.TurnBasedModeActive || Game.Instance.Controllers.TurnController.IsPlayerTurn))
		{
			base.ViewModel.SetActive(isActive: true);
		}
	}

	private void CreateInput()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(null, null, new Vector2Int(1, 0));
		m_InputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "SurfaceActionBarPartAbilitiesConsoleView"
		});
		m_MoveAbilityInputLayer = m_NavigationBehaviour.GetInputLayer(new InputLayer
		{
			ContextName = "MoveAbility"
		});
		InputBindStruct inputBindStruct = m_InputLayer.AddButton(OnDecline, 9, base.ViewModel.IsActive);
		inputBindStruct.AddTo(this);
		m_HintsWidget.BindHint(inputBindStruct, UIStrings.Instance.CommonTexts.Cancel).AddTo(this);
		m_InputLayer.AddButton(OnDecline, 11, base.ViewModel.IsActive).AddTo(this);
		InputBindStruct inputBindStruct2 = m_InputLayer.AddButton(ToggleTooltip, 19, m_HasTooltip, InputActionEventType.ButtonJustReleased);
		inputBindStruct2.AddTo(this);
		m_HintsWidget.BindHint(inputBindStruct2, UIStrings.Instance.CommonTexts.Information).AddTo(this);
		InputBindStruct inputBindStruct3 = m_InputLayer.AddButton(ShowContextMenu, 10, m_HasContextMenu);
		inputBindStruct3.AddTo(this);
		m_HintsWidget.BindHint(inputBindStruct3, UIStrings.Instance.ContextMenu.ContextMenu).AddTo(this);
		InputBindStruct inputBindStruct4 = m_MoveAbilityInputLayer.AddButton(delegate
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.DeleteSlot(m_CurrentIndex);
			});
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.SetMoveAbilityMode(on: false);
			});
		}, 9, base.ViewModel.MoveAbilityMode);
		inputBindStruct4.AddTo(this);
		m_HintsWidget.BindHint(inputBindStruct4, UIStrings.Instance.CommonTexts.Cancel).AddTo(this);
		InputBindStruct inputBindStruct5 = m_MoveAbilityInputLayer.AddButton(delegate
		{
		}, 8, base.ViewModel.MoveAbilityMode);
		inputBindStruct5.AddTo(this);
		m_HintsWidget.BindHint(inputBindStruct5, UIStrings.Instance.ActionTexts.MoveItem).AddTo(this);
		m_PageNavigation.AddInput(m_MoveAbilityInputLayer, base.ViewModel.MoveAbilityMode);
		m_PageNavigation.AddInput(m_InputLayer, base.ViewModel.IsActive);
		m_NavigationBehaviour.Focus.Subscribe(OnFocusEntity).AddTo(this);
	}

	public void AddInputToPages(InputLayer inputLayer, ReactiveProperty<bool> active)
	{
		m_PageNavigation.AddInput(inputLayer, active);
		m_ShowTooltip = false;
	}

	private void OnActive(bool active)
	{
		if (active)
		{
			m_MoveAnimator.Or(null)?.AppearAnimation();
			Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
			GamePad.Instance.PushLayer(m_InputLayer);
			GridConsoleNavigationBehaviour navigationBehaviour = m_NavigationBehaviour;
			IConsoleNavigationEntity entity;
			if (!(m_LastAbilitySlot != null))
			{
				entity = m_Row.GetFirstValidEntity();
			}
			else
			{
				IConsoleNavigationEntity lastAbilitySlot = m_LastAbilitySlot;
				entity = lastAbilitySlot;
			}
			navigationBehaviour.FocusOnEntityManual(entity);
		}
		else
		{
			m_MoveAnimator.Or(null)?.DisappearAnimation();
			GamePad.Instance.PopLayer(m_InputLayer);
			m_NavigationBehaviour.UnFocusCurrentEntity();
		}
	}

	private void OnDecline(InputActionEventData data)
	{
		base.ViewModel.SetActive(isActive: false);
		m_LastAbilitySlot = null;
	}

	private void OnMoveMode(bool on)
	{
		if (on)
		{
			GamePad.Instance.PushLayer(m_MoveAbilityInputLayer);
		}
		else
		{
			GamePad.Instance.PopLayer(m_MoveAbilityInputLayer);
		}
	}

	private void OnFocusEntity(IConsoleEntity entity)
	{
		m_CurrentAbilitySlot = entity as ActionBarSlotAbilityConsoleView;
		if (entity is ActionBarSlotAbilityConsoleView lastAbilitySlot)
		{
			m_LastAbilitySlot = lastAbilitySlot;
		}
		m_HasContextMenu.Value = (bool)m_CurrentAbilitySlot && m_CurrentAbilitySlot.Index != -1;
		TooltipBaseTemplate tooltipBaseTemplate = (entity as IHasTooltipTemplate)?.TooltipTemplate();
		m_HasTooltip.Value = tooltipBaseTemplate != null;
		if (m_ShowTooltip)
		{
			((entity as MonoBehaviour) ?? (entity as IMonoBehaviour)?.MonoBehaviour).ShowTooltip(tooltipBaseTemplate, new TooltipConfig
			{
				TooltipPlace = m_TooltipPlace,
				PriorityPivots = new List<Vector2>
				{
					new Vector2(1f, 0f)
				}
			});
		}
		else
		{
			TooltipHelper.HideTooltip();
		}
		if (base.ViewModel.MoveAbilityMode.CurrentValue && (bool)m_CurrentAbilitySlot && m_CurrentMechanicSlot != null)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(m_CurrentMechanicSlot, m_CurrentIndex, m_CurrentAbilitySlot.Index);
			});
		}
		if ((bool)m_CurrentAbilitySlot)
		{
			m_CurrentMechanicSlot = m_CurrentAbilitySlot.MechanicActionBarSlot;
			m_CurrentIndex = m_CurrentAbilitySlot.Index;
		}
	}

	private void ToggleTooltip(InputActionEventData data)
	{
		m_ShowTooltip = !RootUIContext.Instance.TooltipIsShown;
		OnFocusEntity(m_NavigationBehaviour.DeepestNestedFocus);
	}

	private void ShowContextMenu(InputActionEventData data)
	{
		if ((bool)m_CurrentAbilitySlot && m_CurrentAbilitySlot.Index != -1)
		{
			m_CurrentAbilitySlot.ShowContextMenu(m_CurrentAbilitySlot.ContextMenuEntities);
		}
	}

	public void HandleRemoveFocus()
	{
		m_CulledFocus = m_NavigationBehaviour.DeepestNestedFocus;
		m_NavigationBehaviour.UnFocusCurrentEntity();
	}

	public void HandleRestoreFocus()
	{
		if (m_CulledFocus != null)
		{
			m_NavigationBehaviour.FocusOnEntityManual(m_CulledFocus);
			m_NavigationBehaviour.UpdateDeepestFocusObserve();
		}
		m_CulledFocus = null;
	}

	public void HandleChooseMode(bool active)
	{
		if (!active)
		{
			(from s in m_Row.GetSlots()
				select s as ActionBarSlotAbilityConsoleView).ForEach(delegate(ActionBarSlotAbilityConsoleView slot)
			{
				slot.SetSelectionActiveState(isActive: false);
			});
		}
	}
}
