using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Items.Equipment;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartWeaponSetVM : ViewModel
{
	public readonly AutoDisposingList<ActionBarSlotVM> MainHandSlots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly AutoDisposingList<ActionBarSlotVM> OffHandSlots = new AutoDisposingList<ActionBarSlotVM>();

	public readonly AutoDisposingList<ActionBarSlotVM> ComboHandsSlots = new AutoDisposingList<ActionBarSlotVM>();

	private readonly ReactiveProperty<ItemSlotVM> m_MainHandWeapon = new ReactiveProperty<ItemSlotVM>();

	private readonly ReactiveProperty<ItemSlotVM> m_OffHandWeapon = new ReactiveProperty<ItemSlotVM>();

	private readonly ReactiveCommand<Unit> m_SlotsUpdated = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<bool> m_IsCurrent = new ReactiveProperty<bool>();

	public int Index;

	public bool IsTwoHanded;

	public HandsEquipmentSet HandSet;

	private Action m_SwitchSetAction;

	public EntityRef<BaseUnitEntity> Unit;

	public ReadOnlyReactiveProperty<ItemSlotVM> MainHandWeapon => m_MainHandWeapon;

	public ReadOnlyReactiveProperty<ItemSlotVM> OffHandWeapon => m_OffHandWeapon;

	public Observable<Unit> SlotsUpdated => m_SlotsUpdated;

	public ReadOnlyReactiveProperty<bool> IsCurrent => m_IsCurrent;

	public List<ActionBarSlotVM> AllSlots
	{
		get
		{
			List<ActionBarSlotVM> list = new List<ActionBarSlotVM>();
			list.AddRange(MainHandSlots);
			list.AddRange(OffHandSlots);
			list.AddRange(ComboHandsSlots);
			return list;
		}
	}

	public void InitForUnit(int index, EntityRef<BaseUnitEntity> unit, HandsEquipmentSet handSet, Action switchWeapon)
	{
		Index = index;
		Unit = unit;
		HandSet = handSet;
		m_SwitchSetAction = switchWeapon;
		UpdateSlots();
	}

	public void UpdateIsCurrent(bool state)
	{
		m_IsCurrent.Value = state;
		UpdateSlots();
	}

	public void UpdateSlots()
	{
		ClearAll();
		IsTwoHanded = (HandSet.PrimaryHand?.MaybeWeapon?.HoldInTwoHands).GetValueOrDefault();
		m_MainHandWeapon.Value = new ItemSlotVM(HandSet.PrimaryHand?.MaybeWeapon, 0, null, compareTooltipEnabled: false).AddTo(this);
		m_OffHandWeapon.Value = new ItemSlotVM(HandSet.SecondaryHand?.MaybeWeapon, 1, null, compareTooltipEnabled: false).AddTo(this);
		if (IsCurrent.CurrentValue)
		{
			if (MainHandSlots.Any((ActionBarSlotVM s) => s.IsFake))
			{
				MainHandSlots.Clear();
			}
			if (OffHandSlots.Any((ActionBarSlotVM s) => s.IsFake))
			{
				OffHandSlots.Clear();
			}
			AddSlotsForHand(HandSet.PrimaryHand, MainHandSlots);
			AddSlotsForHand(HandSet.SecondaryHand, OffHandSlots);
		}
		else
		{
			if (MainHandSlots.Empty())
			{
				AddSlotsSceletons(HandSet.PrimaryHand, MainHandSlots);
			}
			if (OffHandSlots.Empty())
			{
				AddSlotsSceletons(HandSet.SecondaryHand, OffHandSlots);
			}
		}
		m_SlotsUpdated.Execute();
	}

	private void AddSlotsForHand(HandSlot hand, IList<ActionBarSlotVM> slots)
	{
		if (hand.MaybeWeapon != null)
		{
			for (int i = 0; i < hand.MaybeWeapon.Abilities.Count; i++)
			{
				TryAddAbilityToSlots(slots, hand.MaybeWeapon.Abilities[i], i);
			}
		}
	}

	private void AddSlotsSceletons(HandSlot hand, IList<ActionBarSlotVM> slots)
	{
		BlueprintItemWeapon blueprintItemWeapon = hand.MaybeWeapon?.Blueprint;
		if (blueprintItemWeapon == null)
		{
			return;
		}
		foreach (WeaponAbility weaponAbility in blueprintItemWeapon.WeaponAbilities)
		{
			ActionBarSlotVM actionBarSlotVM = new ActionBarSlotVM(weaponAbility, hand.MaybeWeapon);
			actionBarSlotVM.AddTo(this);
			slots.Add(actionBarSlotVM);
		}
	}

	private void TryAddAbilityToSlots(IList<ActionBarSlotVM> slots, Ability ability, int index)
	{
		if (slots.Any((ActionBarSlotVM s) => (s.MechanicActionBarSlot as MechanicActionBarSlotAbility).Ability == ability.Data))
		{
			return;
		}
		if (ability.MaybeData == null)
		{
			PFLog.Ability.Error(ability.Name + " was removed/detached");
			return;
		}
		MechanicActionBarSlotAbility mechanicActionBarSlotAbility = new MechanicActionBarSlotAbility
		{
			Ability = ability.Data,
			Unit = Unit
		};
		if (mechanicActionBarSlotAbility.IsBad())
		{
			PFLog.Ability.Error(ability.Name + " lost Owner in its Fact");
			return;
		}
		ActionBarSlotVM actionBarSlotVM = new ActionBarSlotVM(mechanicActionBarSlotAbility, index);
		actionBarSlotVM.AddTo(this);
		slots.Add(actionBarSlotVM);
	}

	protected override void OnDispose()
	{
		ClearAll();
	}

	public void ClearAll()
	{
		MainHandSlots.Clear();
		OffHandSlots.Clear();
		ComboHandsSlots.Clear();
	}

	public void SwitchWeapon()
	{
		m_SwitchSetAction();
	}
}
