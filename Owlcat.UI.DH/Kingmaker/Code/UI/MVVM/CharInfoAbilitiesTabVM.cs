using System.Collections.Generic;
using System.Linq;
using Code.View.UI.UIUtils;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.MVVM.ServiceWindows.AbilityModification;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.Parts;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility.DotNetExtensions;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoAbilitiesTabVM : CharInfoComponentVM
{
	private ReactiveProperty<BaseUnitEntity> m_Unit = new ReactiveProperty<BaseUnitEntity>();

	private ReactiveProperty<List<AbilitySlotVM>> m_AbilitiesVMCollection = new ReactiveProperty<List<AbilitySlotVM>>();

	private ReactiveProperty<List<AbilitySlotVM>> m_AttackAbilitiesVMCollection = new ReactiveProperty<List<AbilitySlotVM>>();

	private ReactiveProperty<List<ModificationSlotVM>> m_ModificationsVMCollection = new ReactiveProperty<List<ModificationSlotVM>>();

	private ReactiveProperty<AbilitySlotVM> m_SelectedAbilitySlot = new ReactiveProperty<AbilitySlotVM>();

	private ReactiveProperty<AbilitySlotVM> m_HoveredAbilitySlot = new ReactiveProperty<AbilitySlotVM>();

	private ReactiveProperty<ModificationSlotVM> m_SelectedModifierSlot = new ReactiveProperty<ModificationSlotVM>();

	private ReactiveProperty<ModificationSlotVM> m_HoveredModifierSlot = new ReactiveProperty<ModificationSlotVM>();

	private ReactiveProperty<PointerEventData> m_ModifierDragData = new ReactiveProperty<PointerEventData>();

	private ReactiveProperty<bool> m_ModifierFilterActive = new ReactiveProperty<bool>();

	private readonly CharacterInfoVM m_characterInfoVM;

	public ActionBarPartConsumablesVM Consumables { get; private set; }

	public ActionBarPartWeaponsVM Weapons { get; private set; }

	public ActionBarPartAbilitiesVM Abilities { get; private set; }

	public ReadOnlyReactiveProperty<List<AbilitySlotVM>> AbilitiesVMCollection => m_AbilitiesVMCollection;

	public ReadOnlyReactiveProperty<List<AbilitySlotVM>> AttackAbilitiesVMCollection => m_AttackAbilitiesVMCollection;

	public ReadOnlyReactiveProperty<List<ModificationSlotVM>> ModificationsVMCollection => m_ModificationsVMCollection;

	public ReadOnlyReactiveProperty<AbilitySlotVM> SelectedAbilitySlot => m_SelectedAbilitySlot;

	public ReadOnlyReactiveProperty<AbilitySlotVM> HoveredAbilitySlot => m_HoveredAbilitySlot;

	public ReadOnlyReactiveProperty<ModificationSlotVM> SelectedModifierSlot => m_SelectedModifierSlot;

	public ReadOnlyReactiveProperty<ModificationSlotVM> HoveredModifierSlot => m_HoveredModifierSlot;

	public ReadOnlyReactiveProperty<PointerEventData> ModifierDragData => m_ModifierDragData;

	public ReadOnlyReactiveProperty<bool> ModifierFilterActive => m_ModifierFilterActive;

	[CanBeNull]
	public PartAbilityModifiers PartAbilityModifiers => m_Unit.CurrentValue?.GetOptional<PartAbilityModifiers>();

	public CharInfoAbilitiesTabVM(ReadOnlyReactiveProperty<BaseUnitEntity> unit, CharacterInfoVM characterInfoVM)
		: base(unit)
	{
		Consumables = new ActionBarPartConsumablesVM().AddTo(this);
		Weapons = new ActionBarPartWeaponsVM().AddTo(this);
		Abilities = new ActionBarPartAbilitiesVM(isInCharScreen: true).AddTo(this);
		Refresh(unit.CurrentValue);
		m_characterInfoVM = characterInfoVM;
	}

	private void Refresh(BaseUnitEntity unit)
	{
		Consumables.SetUnit(unit);
		Weapons.SetUnit(unit);
		Abilities.SetUnit(unit);
		m_SelectedAbilitySlot.Value = null;
		m_SelectedModifierSlot.Value = null;
		m_Unit.Value = unit;
		List<AbilitySlotVM> list = (from ability in unit.Abilities.Visible
			where !ability.Blueprint.IsWeaponAbility
			select new AbilitySlotVM(ability, this)).ToList();
		list.AddRange(unit.ToggleAbilities.Visible.Select((ToggleAbility t) => new AbilitySlotVM(t, this)).ToList());
		m_AbilitiesVMCollection.Value = list;
		List<AbilitySlotVM> value = new List<AbilitySlotVM>
		{
			new AbilitySlotVM(ConfigRoot.Instance.AbilityRoot.WeaponSingleShotTag, this),
			new AbilitySlotVM(ConfigRoot.Instance.AbilityRoot.WeaponBurstTag, this),
			new AbilitySlotVM(ConfigRoot.Instance.AbilityRoot.WeaponAoETag, this)
		};
		m_AttackAbilitiesVMCollection.Value = value;
		if (PartAbilityModifiers != null)
		{
			m_ModificationsVMCollection.Value = PartAbilityModifiers.KnownModifiers.Select((BlueprintAbilityModifier modifier) => new ModificationSlotVM(modifier, this, unit)).ToList();
		}
		if (unit != null)
		{
			EventBus.RaiseEvent(delegate(ICharInfoAbilitiesOpenHandler h)
			{
				h.HandleCharInfoAbilitiesOpen(unit);
			});
		}
	}

	protected override void RefreshData()
	{
		Refresh(Unit.CurrentValue);
	}

	public void SelectAbilitySlot(AbilitySlotVM slot)
	{
		if (m_SelectedModifierSlot.Value != null)
		{
			IntegrateModifier(slot, m_SelectedModifierSlot.Value);
			m_SelectedModifierSlot.Value = null;
		}
		else
		{
			m_SelectedAbilitySlot.Value = ((m_SelectedAbilitySlot.Value == slot) ? null : slot);
		}
	}

	public void SelectModifier(ModificationSlotVM slot)
	{
		if (m_SelectedAbilitySlot.Value != null)
		{
			IntegrateModifier(m_SelectedAbilitySlot.Value, slot);
			m_SelectedAbilitySlot.Value = null;
		}
		else
		{
			m_SelectedModifierSlot.Value = ((m_SelectedModifierSlot.Value == slot) ? null : slot);
		}
	}

	public void HoverModifier(ModificationSlotVM slot, bool isHover)
	{
		m_HoveredModifierSlot.Value = (isHover ? slot : ((m_SelectedModifierSlot.Value == slot) ? null : m_SelectedModifierSlot.Value));
	}

	public void TryIntegrateModifier(ModificationSlotVM slot)
	{
		AbilitySlotVM abilitySlotVM = FindFirstAvailableAbility(slot.Modifier);
		if (abilitySlotVM != null)
		{
			IntegrateModifier(abilitySlotVM, slot);
		}
		m_SelectedModifierSlot.Value = null;
	}

	public void IntegrateModifier(AbilitySlotVM abilitySlot, ModificationSlotVM modifierSlot)
	{
		if (abilitySlot == null || modifierSlot == null || PartAbilityModifiers == null)
		{
			return;
		}
		if (UIUtilityCombat.IsCombatLockActive())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, null, addToLog: false, WarningNotificationFormat.Warning);
			});
		}
		else if (abilitySlot.IsSuitableModifier(modifierSlot.Modifier))
		{
			DetachModifier(abilitySlot);
			FreeModifier(modifierSlot);
			abilitySlot.SetModifier(modifierSlot.Modifier);
			modifierSlot.UpdateEquipStatus();
			if (abilitySlot.Ability != null)
			{
				PartAbilityModifiers.AddModifier(modifierSlot.Modifier, abilitySlot.Ability.Blueprint);
				m_characterInfoVM.CheckModifiers();
			}
			else if (abilitySlot.TargetTag != null)
			{
				PartAbilityModifiers.AddModifier(modifierSlot.Modifier, abilitySlot.TargetTag);
				m_characterInfoVM.CheckModifiers();
			}
			else if (abilitySlot.ToggleAbility != null)
			{
				PartAbilityModifiers.BindModifier(modifierSlot.Modifier, abilitySlot.ToggleAbility.Blueprint);
				m_characterInfoVM.CheckModifiers();
			}
		}
	}

	public void DetachModifier(AbilitySlotVM abilitySlot)
	{
		if (abilitySlot?.AppliedModifier.CurrentValue == null || PartAbilityModifiers == null)
		{
			return;
		}
		if (UIUtilityCombat.IsCombatLockActive())
		{
			EventBus.RaiseEvent(delegate(IWarningNotificationUIHandler h)
			{
				h.HandleWarning(WarningNotificationType.EquipInCombatIsImpossible, null, addToLog: false, WarningNotificationFormat.Warning);
			});
			return;
		}
		BlueprintAbilityModifier modifier = abilitySlot.AppliedModifier.CurrentValue;
		abilitySlot.SetModifier(null);
		m_ModificationsVMCollection.Value.FirstOrDefault((ModificationSlotVM m) => m.Modifier == modifier)?.UpdateEquipStatus();
		if (abilitySlot.Ability != null && PartAbilityModifiers.IsAddedManually(abilitySlot.Ability.Blueprint, modifier))
		{
			PartAbilityModifiers.RemoveModifier(abilitySlot.Ability.Blueprint, modifier);
		}
		if (abilitySlot.TargetTag != null && PartAbilityModifiers.IsAddedManually(abilitySlot.TargetTag, modifier))
		{
			PartAbilityModifiers.RemoveModifier(abilitySlot.TargetTag, modifier);
		}
		if (abilitySlot.ToggleAbility != null && PartAbilityModifiers.GetBoundModifiers(abilitySlot.ToggleAbility.Blueprint).Contains(modifier))
		{
			PartAbilityModifiers.UnbindModifier(modifier, abilitySlot.ToggleAbility.Blueprint);
		}
		m_characterInfoVM.CheckModifiers();
	}

	public BlueprintAbilityModifier GetAbilityModifier(AbilitySlotVM abilitySlot)
	{
		if (PartAbilityModifiers == null)
		{
			return null;
		}
		if (abilitySlot.Ability != null)
		{
			return PartAbilityModifiers.AddedModifiers.FirstOrDefault((PartAbilityModifiers.AddedEntry value) => value.IsAddedManually && value.Ability == abilitySlot.Ability.Blueprint)?.Modifier;
		}
		if (abilitySlot.TargetTag != null)
		{
			return PartAbilityModifiers.AddedModifiers.FirstOrDefault((PartAbilityModifiers.AddedEntry value) => value.IsAddedManually && value.AbilityTag == abilitySlot.TargetTag)?.Modifier;
		}
		if (abilitySlot.ToggleAbility != null)
		{
			return PartAbilityModifiers.GetBoundModifiers(abilitySlot.ToggleAbility.Blueprint)?.FirstOrDefault();
		}
		return null;
	}

	public bool IsModifierEquipped(ModificationSlotVM modifierSlot)
	{
		return GetAbilityWithModifier(modifierSlot.Modifier) != null;
	}

	public void ModifierDrag(PointerEventData eventData, bool isDragEnd, AbilitySlotVM abilitySlot)
	{
		ModificationSlotVM modifierSlot = m_ModificationsVMCollection.Value.FirstOrDefault((ModificationSlotVM m) => m.Modifier == abilitySlot.AppliedModifier.CurrentValue);
		ModifierDrag(eventData, isDragEnd, modifierSlot);
	}

	public void ModifierDrag(PointerEventData eventData, bool isDragEnd, ModificationSlotVM modifierSlot)
	{
		if (isDragEnd)
		{
			m_ModifierDragData.Value = null;
			m_SelectedModifierSlot.Value = null;
			if (m_HoveredAbilitySlot.Value != null)
			{
				IntegrateModifier(m_HoveredAbilitySlot.Value, modifierSlot);
			}
			else
			{
				FreeModifier(modifierSlot);
			}
		}
		else if (!(eventData.delta == Vector2.zero))
		{
			if (m_SelectedModifierSlot.Value != modifierSlot)
			{
				m_SelectedModifierSlot.Value = modifierSlot;
			}
			m_ModifierDragData.Value = eventData;
			m_ModifierDragData.ForceNotify();
		}
	}

	public void OnHoverAbilitySlot(AbilitySlotVM abilitySlotVM)
	{
		m_HoveredAbilitySlot.Value = abilitySlotVM;
	}

	public void ToggleModifierFilter()
	{
		m_ModifierFilterActive.Value = !m_ModifierFilterActive.Value;
	}

	private void FreeModifier(ModificationSlotVM modifierSlot)
	{
		if (modifierSlot.IsEquipped.CurrentValue)
		{
			AbilitySlotVM abilityWithModifier = GetAbilityWithModifier(modifierSlot.Modifier);
			if (abilityWithModifier != null)
			{
				DetachModifier(abilityWithModifier);
			}
		}
	}

	private AbilitySlotVM GetAbilityWithModifier(BlueprintAbilityModifier modifier)
	{
		return m_AttackAbilitiesVMCollection.Value.Concat(m_AbilitiesVMCollection.Value).FirstOrDefault((AbilitySlotVM a) => a.AppliedModifier.CurrentValue == modifier);
	}

	private AbilitySlotVM FindFirstAvailableAbility(BlueprintAbilityModifier modifier)
	{
		return m_AttackAbilitiesVMCollection.Value.Concat(m_AbilitiesVMCollection.Value).FirstOrDefault((AbilitySlotVM a) => a.AppliedModifier.CurrentValue == null && a.IsSuitableModifier(modifier));
	}
}
