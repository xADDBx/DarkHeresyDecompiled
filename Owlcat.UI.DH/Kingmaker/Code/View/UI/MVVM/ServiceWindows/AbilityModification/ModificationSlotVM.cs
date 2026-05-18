using System.Linq;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.View.UI.MVVM.ServiceWindows.AbilityModification;

public class ModificationSlotVM : ViewModel
{
	public readonly BlueprintAbilityModifier Modifier;

	public readonly string ModifierName;

	public readonly string Tag;

	public readonly Sprite ModifierIcon;

	public readonly TooltipBaseTemplate Tooltip;

	private ReactiveProperty<bool> m_IsSelected = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> m_IsSuitable = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> m_IsEquipped = new ReactiveProperty<bool>();

	private ReactiveProperty<bool> m_IsSuitedAbilityHover = new ReactiveProperty<bool>();

	private readonly CharInfoAbilitiesTabVM m_AbilitiesTabVM;

	public ReadOnlyReactiveProperty<bool> IsSelected => m_IsSelected;

	public ReadOnlyReactiveProperty<bool> IsSuitable => m_IsSuitable;

	public ReadOnlyReactiveProperty<bool> IsEquipped => m_IsEquipped;

	public ReadOnlyReactiveProperty<bool> IsSuitedAbilityHover => m_IsSuitedAbilityHover;

	public ModificationSlotVM(BlueprintAbilityModifier modifier, CharInfoAbilitiesTabVM abilitiesTabVM, BaseUnitEntity caster = null)
	{
		Modifier = modifier;
		ModifierName = modifier.Name;
		BlueprintAbilityTag blueprintAbilityTag = modifier.Tags.FirstOrDefault();
		Tag = blueprintAbilityTag?.Name.Text;
		ModifierIcon = blueprintAbilityTag?.Icon.GetDefaultIfNull(DefaultImageType.Modifier) ?? UIUtilityImage.GetDefault(DefaultImageType.Modifier);
		m_AbilitiesTabVM = abilitiesTabVM;
		m_AbilitiesTabVM.SelectedModifierSlot.Subscribe(delegate(ModificationSlotVM s)
		{
			m_IsSelected.Value = s == this;
		}).AddTo(this);
		m_AbilitiesTabVM.SelectedAbilitySlot.Subscribe(delegate
		{
			UpdateSuitability();
		}).AddTo(this);
		m_AbilitiesTabVM.ModifierFilterActive.Subscribe(delegate
		{
			UpdateSuitability();
		}).AddTo(this);
		m_AbilitiesTabVM.HoveredAbilitySlot.Subscribe(delegate
		{
			OnAbilityHover();
		}).AddTo(this);
		UpdateEquipStatus();
		Tooltip = new TooltipTemplateLevelUpModifier(modifier, null, caster);
	}

	public void OnClick()
	{
		m_AbilitiesTabVM.SelectModifier(this);
	}

	public void OnDoubleClick()
	{
		m_AbilitiesTabVM.TryIntegrateModifier(this);
	}

	public void OnHover(bool isHover)
	{
		m_AbilitiesTabVM.HoverModifier(this, isHover);
	}

	public void UpdateEquipStatus()
	{
		m_IsEquipped.Value = m_AbilitiesTabVM.IsModifierEquipped(this);
	}

	public void OnDrag(PointerEventData eventData, bool isDragEnd = false)
	{
		m_AbilitiesTabVM.ModifierDrag(eventData, isDragEnd, this);
	}

	private void OnAbilityHover()
	{
		AbilitySlotVM currentValue = m_AbilitiesTabVM.HoveredAbilitySlot.CurrentValue;
		if (currentValue == null)
		{
			m_IsSuitedAbilityHover.Value = false;
		}
		else
		{
			m_IsSuitedAbilityHover.Value = currentValue.AppliedModifier.CurrentValue == Modifier || (currentValue.IsSuitableModifier(Modifier) && currentValue.AppliedModifier.CurrentValue == null);
		}
	}

	private void UpdateSuitability()
	{
		ReactiveProperty<bool> isSuitable = m_IsSuitable;
		AbilitySlotVM currentValue = m_AbilitiesTabVM.SelectedAbilitySlot.CurrentValue;
		isSuitable.Value = currentValue == null || currentValue.IsSuitableModifier(Modifier) || !m_AbilitiesTabVM.ModifierFilterActive.CurrentValue;
	}
}
