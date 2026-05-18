using Code.View.UI.MVVM.Tooltip.Templates;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows.AbilityModification;

public class AbilitySlotVM : ViewModel
{
	public readonly Ability Ability;

	public readonly ToggleAbility ToggleAbility;

	public readonly BlueprintAbilityTag TargetTag;

	public readonly string Name;

	public readonly Sprite Icon;

	private readonly CharInfoAbilitiesTabVM m_AbilitiesTabVM;

	private ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	private ReactiveProperty<bool?> m_CanApplyModifierOnSelect = new ReactiveProperty<bool?>();

	private ReactiveProperty<bool> m_CanApplyModifierOnHover = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> m_HasThisModifier = new ReactiveProperty<bool>();

	private ReactiveProperty<BlueprintAbilityModifier> m_AppliedModifier = new ReactiveProperty<BlueprintAbilityModifier>();

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public ReadOnlyReactiveProperty<bool?> CanApplyModifierOnSelect => m_CanApplyModifierOnSelect;

	public ReadOnlyReactiveProperty<bool> CanApplyModifierOnHover => m_CanApplyModifierOnHover;

	public ReadOnlyReactiveProperty<bool> HasThisModifier => m_HasThisModifier;

	public ReadOnlyReactiveProperty<BlueprintAbilityModifier> AppliedModifier => m_AppliedModifier;

	public TooltipBaseTemplate Tooltip { get; private set; }

	public TooltipBaseTemplate ModifierTooltip { get; private set; }

	public bool IsBrokenOrHeroic
	{
		get
		{
			BlueprintAbilityWrapper blueprintAbilityWrapper = Ability?.Data.Blueprint;
			if (blueprintAbilityWrapper != null)
			{
				if (!blueprintAbilityWrapper.IsBroken)
				{
					return blueprintAbilityWrapper.IsHeroic;
				}
				return true;
			}
			return false;
		}
	}

	[CanBeNull]
	private PartAbilityModifiers PartAbilityModifiers => m_AbilitiesTabVM.PartAbilityModifiers;

	public AbilitySlotVM(Ability ability, CharInfoAbilitiesTabVM abilitiesTabVM)
	{
		Ability = ability;
		Name = ability.Name;
		Icon = ability.Icon;
		m_AbilitiesTabVM = abilitiesTabVM;
		Tooltip = new TooltipTemplateAbility(ability.Data);
		Init();
	}

	public AbilitySlotVM(BlueprintAbilityTag tag, CharInfoAbilitiesTabVM abilitiesTabVM)
	{
		TargetTag = tag;
		Name = tag.Name;
		Icon = tag.Icon;
		m_AbilitiesTabVM = abilitiesTabVM;
		Tooltip = new TooltipTemplateAbilityTag(tag);
		Init();
	}

	public AbilitySlotVM(ToggleAbility toggleAbility, CharInfoAbilitiesTabVM abilitiesTabVM)
	{
		ToggleAbility = toggleAbility;
		Name = toggleAbility.Name;
		Icon = toggleAbility.Icon;
		m_AbilitiesTabVM = abilitiesTabVM;
		Tooltip = new TooltipTemplateToggleAbility(toggleAbility);
		Init();
	}

	public bool IsSuitableModifier(BlueprintAbilityModifier modifier)
	{
		PartAbilityModifiers partAbilityModifiers = PartAbilityModifiers;
		if (partAbilityModifiers == null || IsBrokenOrHeroic)
		{
			return false;
		}
		if (Ability != null)
		{
			return partAbilityModifiers.IsSuitableModifier(modifier, Ability);
		}
		if (ToggleAbility != null)
		{
			return partAbilityModifiers.IsSuitableModifier(modifier, ToggleAbility);
		}
		if (TargetTag != null)
		{
			return partAbilityModifiers.IsSuitableModifier(modifier, ConfigRoot.Instance.AbilityRoot.AttackAbilityTag);
		}
		return false;
	}

	public void OnClick()
	{
		m_AbilitiesTabVM.SelectAbilitySlot(this);
	}

	public void OnHover(bool value)
	{
		m_AbilitiesTabVM.OnHoverAbilitySlot(value ? this : null);
	}

	public void SetModifier(BlueprintAbilityModifier modifier)
	{
		ModifierTooltip = ((modifier == null) ? null : new TooltipTemplateLevelUpModifier(modifier, null, m_AbilitiesTabVM.Unit.CurrentValue));
		m_AppliedModifier.Value = modifier;
	}

	public void OnDrag(PointerEventData eventData, bool isDragEnd = false)
	{
		if (m_AppliedModifier.Value != null)
		{
			m_AbilitiesTabVM.ModifierDrag(eventData, isDragEnd, this);
		}
	}

	private void Init()
	{
		SetModifier(m_AbilitiesTabVM.GetAbilityModifier(this));
		m_AbilitiesTabVM.SelectedAbilitySlot.Subscribe(delegate(AbilitySlotVM slot)
		{
			m_IsSelected.Value = slot == this;
		}).AddTo(this);
		m_AbilitiesTabVM.SelectedModifierSlot.Subscribe(delegate(ModificationSlotVM m)
		{
			m_CanApplyModifierOnSelect.Value = ((m == null) ? null : new bool?(IsSuitableModifier(m.Modifier)));
		}).AddTo(this);
		m_AbilitiesTabVM.HoveredModifierSlot.Subscribe(delegate(ModificationSlotVM m)
		{
			m_CanApplyModifierOnHover.Value = m != null && IsSuitableModifier(m.Modifier);
		}).AddTo(this);
		m_AbilitiesTabVM.HoveredModifierSlot.Subscribe(delegate(ModificationSlotVM m)
		{
			m_HasThisModifier.Value = m != null && m_AppliedModifier.Value == m.Modifier;
		}).AddTo(this);
	}
}
