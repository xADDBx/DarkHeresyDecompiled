using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityAppliedModifierVM : ViewModel
{
	public readonly Sprite Icon;

	public readonly Color BackgroundColor;

	public readonly string Description;

	public readonly bool IsLocked;

	public readonly TooltipBaseTemplate Tooltip;

	public AbilityAppliedModifierVM(BlueprintAbilityModifier abilityModifier, BlueprintAbilityTag tagBlueprint, MechanicEntity caster, bool isManuallyAdded)
	{
		Icon = tagBlueprint.SymbolIcon;
		BackgroundColor = tagBlueprint.BackgroundColor;
		Description = abilityModifier.Name;
		IsLocked = !isManuallyAdded;
		Tooltip = new TooltipTemplateLevelUpModifier(abilityModifier, null, caster as BaseUnitEntity);
	}
}
