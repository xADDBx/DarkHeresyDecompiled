using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Abilities;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipElementAbilityWithModifierVM : ViewModel
{
	public readonly Ability Ability;

	public readonly ToggleAbility ToggleAbility;

	public readonly BlueprintAbilityTag TargetTag;

	public readonly string Name;

	public readonly string Tags;

	public readonly string Cost;

	public readonly Sprite Icon;

	public readonly Sprite ModifierIcon;

	public TooltipBaseTemplate Tooltip { get; private set; }

	public TooltipElementAbilityWithModifierVM(Ability ability, PartAbilityModifiers partAbilityModifiers)
	{
		Ability = ability;
		Name = ability.Name;
		Icon = ability.Icon;
		Tags = string.Join(", ", ability.Blueprint.Tags.Select((BpRef<BlueprintAbilityTag> t) => t.Blueprint.Name.Text).ToList());
		Cost = string.Format(UIStrings.Instance.Tooltips.AbilityAPCost.Text, ability.Data.GetBaseActionPointCost());
		ModifierIcon = (partAbilityModifiers.AddedModifiers.FirstOrDefault((PartAbilityModifiers.AddedEntry value) => value.IsAddedManually && value.Ability == ability.Blueprint)?.Modifier)?.Tags.FirstOrDefault()?.AbilityIcon;
		Tooltip = new TooltipTemplateAbility(ability.Data);
	}

	public TooltipElementAbilityWithModifierVM(BlueprintAbilityTag tag, PartAbilityModifiers partAbilityModifiers)
	{
		TargetTag = tag;
		Name = tag.Name;
		Icon = tag.Icon;
		Tags = ConfigRoot.Instance.AbilityRoot.AttackAbilityTag.Blueprint.Name.Text;
		ModifierIcon = (partAbilityModifiers.AddedModifiers.FirstOrDefault((PartAbilityModifiers.AddedEntry value) => value.IsAddedManually && value.AbilityTag == tag)?.Modifier)?.Tags.FirstOrDefault()?.AbilityIcon;
		Tooltip = new TooltipTemplateSimple(Name, tag.Description);
	}

	public TooltipElementAbilityWithModifierVM(ToggleAbility toggleAbility, PartAbilityModifiers partAbilityModifiers)
	{
		ToggleAbility = toggleAbility;
		Name = toggleAbility.Name;
		Icon = toggleAbility.Icon;
		Tags = string.Join(", ", toggleAbility.Blueprint.AbilityModifierTags.Select((BpRef<BlueprintAbilityTag> t) => t.Blueprint.Name.Text).ToList());
		ModifierIcon = (partAbilityModifiers.GetBoundModifiers(toggleAbility.Blueprint)?.FirstOrDefault())?.Tags.FirstOrDefault()?.AbilityIcon;
		Tooltip = new TooltipTemplateToggleAbility(toggleAbility);
	}
}
