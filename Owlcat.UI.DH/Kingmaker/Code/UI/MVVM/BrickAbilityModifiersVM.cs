using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Abilities.Blueprints;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityModifiersVM : TooltipBrickVM
{
	public readonly string HeaderText;

	public readonly IReadOnlyList<AbilityAppliedModifierVM> Modifiers;

	public BrickAbilityModifiersVM(IReadOnlyList<BlueprintAbilityModifier> modifiers, BlueprintAbilityModifier manualModifier, MechanicEntity caster)
	{
		HeaderText = UIStrings.Instance.Tooltips.AttachedModifiers;
		List<AbilityAppliedModifierVM> list = new List<AbilityAppliedModifierVM>();
		foreach (BlueprintAbilityModifier modifier in modifiers)
		{
			BlueprintAbilityTag blueprintAbilityTag = modifier.Tags.FirstOrDefault();
			if (blueprintAbilityTag != null)
			{
				bool isManuallyAdded = manualModifier != null && modifier == manualModifier;
				list.Add(new AbilityAppliedModifierVM(modifier, blueprintAbilityTag, caster, isManuallyAdded));
			}
		}
		Modifiers = list;
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		foreach (AbilityAppliedModifierVM modifier in Modifiers)
		{
			modifier.Dispose();
		}
	}
}
