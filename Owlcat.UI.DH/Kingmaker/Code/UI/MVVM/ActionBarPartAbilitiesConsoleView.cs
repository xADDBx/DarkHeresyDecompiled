using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.UI.Common.PageNavigation;
using Kingmaker.GameModes;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common.Animations;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartAbilitiesConsoleView : ActionBarPartAbilitiesBaseView, ICharInfoAbilitiesChooseModeHandler, ISubscriber
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
	private HintView m_ActivateHint;

	[SerializeField]
	private HintView m_ActivateInCombatHint;

	[SerializeField]
	private VeilThicknessConsoleView m_VeilThicknessConsoleView;

	[SerializeField]
	private AbilitySelectorWindowConsoleView m_AbilitySelectorWindowConsoleView;

	private readonly ReactiveProperty<int> m_CurrentRowIndex = new ReactiveProperty<int>();

	private bool m_ShowTooltip = true;

	private readonly ReactiveProperty<bool> m_HasTooltip = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_HasContextMenu = new ReactiveProperty<bool>();

	private ActionBarSlotAbilityConsoleView m_CurrentAbilitySlot;

	private ActionBarSlotAbilityConsoleView m_LastAbilitySlot;

	private MechanicActionBarSlot m_CurrentMechanicSlot;

	private int m_CurrentIndex;

	public override void Initialize()
	{
		m_MoveAnimator.Or(null)?.Initialize();
		m_AbilitySelectorWindowConsoleView.Or(null)?.Initialize();
	}

	protected override void OnBind()
	{
		CreateInput();
		m_CurrentRowIndex.Subscribe(OnIndexChanged).AddTo(this);
		base.ViewModel.UnitChanged.Subscribe(OnUnitChanged).AddTo(this);
		base.ViewModel.SlotCountChanged.Subscribe(OnUnitChanged).AddTo(this);
		base.ViewModel.IsActive.Subscribe(OnActive).AddTo(this);
		if ((bool)m_AbilitySelectorWindowConsoleView)
		{
			base.ViewModel.AbilitySelectorWindowVM.Subscribe(m_AbilitySelectorWindowConsoleView.Bind).AddTo(this);
		}
		base.ViewModel.AbilitySelectorWindowVM.Subscribe(delegate(AbilitySelectorWindowVM value)
		{
			if (value == null && (bool)m_LastAbilitySlot)
			{
				m_LastAbilitySlot = null;
			}
		}).AddTo(this);
		EventBus.Subscribe(this).AddTo(this);
		OnUnitChanged();
	}

	protected override void OnUnbind()
	{
		m_PageNavigation.Dispose();
		OnActive(active: false);
		m_ShowTooltip = true;
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
		List<ActionBarSlotVM> list = new List<ActionBarSlotVM>();
		for (int i = base.ViewModel.RowIndex * base.SlotsInRow; i < (base.ViewModel.RowIndex + 1) * base.SlotsInRow; i++)
		{
			list.Add(base.ViewModel.Slots[i]);
		}
		m_Row.DrawEntries(list, m_SlotView);
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

	public void AddInput()
	{
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
	}

	private void OnActive(bool active)
	{
		if (active)
		{
			m_MoveAnimator.Or(null)?.AppearAnimation();
			Game.Instance.Controllers.ClickEventsController.ClearPointerMode();
		}
		else
		{
			m_MoveAnimator.Or(null)?.DisappearAnimation();
		}
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
