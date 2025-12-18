using System;
using System.Linq;
using Kingmaker.Code.Framework;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Gameplay.Parts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartAbilitiesVM : ActionBarBasePartVM, IActionBarPartAbilitiesHandler, ISubscriber, IClickMechanicActionBarSlotHandler, IEntityGainFactHandler, ISubscriber<IMechanicEntity>, IEntityLostFactHandler, IEntitySubscriber, ITurnBasedModeHandler, IAbilityReplacementsUpdatedHandler
{
	public readonly bool IsInCharScreen;

	public readonly AutoDisposingList<ActionBarSlotVM> Slots = new AutoDisposingList<ActionBarSlotVM>();

	private readonly ReactiveCommand<Unit> m_SlotCountChanged = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<ActionBarSlotVM> m_ChooseAbilitySlot = new ReactiveProperty<ActionBarSlotVM>();

	private readonly ReactiveProperty<bool> m_IsActive = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<AbilitySelectorWindowVM> m_AbilitySelectorWindowVM = new ReactiveProperty<AbilitySelectorWindowVM>();

	private readonly ReactiveProperty<bool> m_MoveAbilityMode = new ReactiveProperty<bool>();

	private IDisposable m_OnAbilitiesChangedDelay;

	public Observable<Unit> SlotCountChanged => m_SlotCountChanged;

	public ReadOnlyReactiveProperty<ActionBarSlotVM> ChooseAbilitySlot => m_ChooseAbilitySlot;

	public ReadOnlyReactiveProperty<bool> IsActive => m_IsActive;

	public int RowIndex
	{
		get
		{
			BaseUnitEntity entity = Unit.Entity;
			if (entity != null)
			{
				_ = entity.ActionBar.SlotRowIndexConsole;
				if (0 == 0)
				{
					BaseUnitEntity entity2 = Unit.Entity;
					if (!(((entity2 != null) ? new int?(entity2.ActionBar.SlotRowIndexConsole * 10) : null) >= Slots.Count))
					{
						return Unit.Entity.ActionBar.SlotRowIndexConsole;
					}
				}
			}
			return 0;
		}
		set
		{
			if (Unit.Entity != null)
			{
				Unit.Entity.ActionBar.SlotRowIndexConsole = value;
			}
		}
	}

	public ReadOnlyReactiveProperty<AbilitySelectorWindowVM> AbilitySelectorWindowVM => m_AbilitySelectorWindowVM;

	public ReadOnlyReactiveProperty<bool> MoveAbilityMode => m_MoveAbilityMode;

	public IEntity GetSubscribingEntity()
	{
		return Unit.Entity;
	}

	public ActionBarPartAbilitiesVM(bool isInCharScreen)
	{
		IsInCharScreen = isInCharScreen;
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		AbilitySelectorWindowVM.CurrentValue?.Dispose();
	}

	protected override void OnUnitChanged()
	{
		ClearSlots();
		FillSlots(30);
		for (int i = 0; i < Unit.Entity.ActionBar.Slots.Count; i++)
		{
			ActionBarSlotVM item = new ActionBarSlotVM(Unit.Entity.ActionBar.GetSlot(i), i, IsInCharScreen, m_MoveAbilityMode);
			Slots.Add(item);
		}
		OnAbilitiesChanged();
	}

	private void UpdateSlots()
	{
		if (Unit.Entity == null)
		{
			return;
		}
		int count = Unit.Entity.ActionBar.Slots.Count;
		bool flag = Slots.Count != count;
		for (int i = 0; i < count; i++)
		{
			MechanicActionBarSlot slot = Unit.Entity.ActionBar.GetSlot(i);
			if (i < Slots.Count)
			{
				Slots[i].SetMechanicSlot(slot);
				continue;
			}
			ActionBarSlotVM item = new ActionBarSlotVM(slot, i, IsInCharScreen, m_MoveAbilityMode);
			Slots.Add(item);
		}
		if (Slots.Count > count)
		{
			Slots.RemoveRangeAndDispose(count, Slots.Count - count);
		}
		if (flag)
		{
			m_SlotCountChanged.Execute();
		}
	}

	protected override void ClearSlots()
	{
		Slots.Clear();
	}

	private void FillSlots(int count)
	{
		Unit.Entity?.ActionBar.GetSlot(count - 1);
	}

	public void MoveSlot(Ability sourceAbility, int targetIndex)
	{
		if (Unit.Entity == null || sourceAbility.Owner != Unit.Entity)
		{
			return;
		}
		MechanicActionBarSlot slot = Unit.Entity.ActionBar.GetSlot(targetIndex);
		if (slot is MechanicActionBarSlotAbility || slot is MechanicActionBarSlotEmpty)
		{
			if (slot is MechanicActionBarSlotAbility mechanicActionBarSlotAbility && mechanicActionBarSlotAbility.Ability == sourceAbility.Data)
			{
				UpdateSlots();
				return;
			}
			Unit.Entity.ActionBar.SetSlot(Unit.Entity, sourceAbility, targetIndex);
			UpdateSlots();
		}
	}

	public void SetSlot(MechanicEntityFact mechanicEntityFact, int targetIndex)
	{
		if (Unit.Entity != null)
		{
			if (mechanicEntityFact is ToggleAbility ability)
			{
				Unit.Entity.ActionBar.SetSlot(Unit.Entity, ability, targetIndex);
			}
			if (mechanicEntityFact is Ability ability2)
			{
				Unit.Entity.ActionBar.SetSlot(Unit.Entity, ability2, targetIndex);
			}
			UpdateSlots();
		}
	}

	public void MoveSlot(MechanicActionBarSlot sourceSlot, int sourceIndex, int targetIndex)
	{
		if (Unit.Entity == null || sourceSlot.Unit != Unit.Entity)
		{
			return;
		}
		MechanicActionBarSlot slot = Unit.Entity.ActionBar.GetSlot(targetIndex);
		if (slot is MechanicActionBarSlotAbility || slot is MechanicActionBarSlotToggleAbility || slot is MechanicActionBarSlotEmpty)
		{
			if (slot == sourceSlot)
			{
				UpdateSlots();
				return;
			}
			Unit.Entity.ActionBar.SetSlot(sourceSlot, targetIndex);
			Unit.Entity.ActionBar.SetSlot(slot, sourceIndex);
			UpdateSlots();
		}
	}

	public void DeleteSlot(int sourceIndex)
	{
		if (Unit.Entity != null)
		{
			Unit.Entity.ActionBar.RemoveSlot(sourceIndex);
			UpdateSlots();
		}
	}

	public void ChooseAbilityToSlot(int targetIndex)
	{
		if (RootUIContext.Instance.CurrentServiceWindow != ServiceWindowsType.CharacterInfo)
		{
			m_IsActive.Value = false;
			m_ChooseAbilitySlot.Value = Slots.ElementAt(targetIndex);
			Slots.ElementAt(targetIndex).SetSelectionBusy(isBusy: true);
			AbilitySelectorWindowVM.CurrentValue?.Dispose();
			m_AbilitySelectorWindowVM.Value = new AbilitySelectorWindowVM(OnConfirm, OnClose, Unit, OnFocus);
		}
		void OnClose()
		{
			m_IsActive.Value = true;
			Slots.ElementAt(targetIndex).SetSelectionBusy(isBusy: false);
			m_ChooseAbilitySlot.Value = null;
			HideSelectionWindow();
		}
		void OnConfirm(CharInfoFeatureVM vm)
		{
			EventBus.RaiseEvent(delegate(IActionBarPartAbilitiesHandler h)
			{
				h.MoveSlot(vm.Ability, targetIndex);
			});
		}
		void OnFocus(CharInfoFeatureVM vm)
		{
			ChooseAbilitySlot.CurrentValue.OverrideIcon(vm.Icon);
		}
	}

	private void HideSelectionWindow()
	{
		AbilitySelectorWindowVM.CurrentValue?.Dispose();
		m_AbilitySelectorWindowVM.Value = null;
	}

	public void SetMoveAbilityMode(bool on)
	{
		m_MoveAbilityMode.Value = on;
		if (!on)
		{
			Slots.ForEach(delegate(ActionBarSlotVM s)
			{
				s.SetSelectionBusy(isBusy: false);
			});
		}
	}

	public void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability)
	{
		m_IsActive.Value = false;
	}

	public void HandleEntityGainFact(EntityFact fact)
	{
		OnAbilitiesChanged();
	}

	public void HandleEntityLostFact(EntityFact fact)
	{
		OnAbilitiesChanged();
	}

	public void HandleAbilityReplacementsUpdated(BlueprintAbility target)
	{
		OnAbilitiesChanged();
	}

	public void SetActive(bool isActive)
	{
		m_IsActive.Value = isActive;
	}

	private void OnAbilitiesChanged()
	{
		if (Unit.Entity != null)
		{
			m_OnAbilitiesChangedDelay?.Dispose();
			m_OnAbilitiesChangedDelay = DelayedInvoker.InvokeInFrames(delegate
			{
				Unit.Entity?.ActionBar.TryToInitialize();
				UpdateSlots();
			}, 1);
			m_OnAbilitiesChangedDelay.AddTo(this);
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			m_IsActive.Value = false;
		}
	}
}
