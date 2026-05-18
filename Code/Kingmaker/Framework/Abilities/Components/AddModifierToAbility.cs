using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Framework.Abilities.Blueprints;
using Kingmaker.Gameplay.Parts;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Framework.Abilities.Components;

[Serializable]
[ComponentName("Modifier/AddModifierToAbility")]
[TypeId("a69607c103b44905879d48515ed52ecb")]
public sealed class AddModifierToAbility : MechanicEntityFactComponentDelegate, IRestrictionProvider
{
	public BpRef<BlueprintAbilityModifier> Modifier = new BpRef<BlueprintAbilityModifier>();

	public bool OnSourceAbility;

	[HideIf("OnSourceAbility")]
	public BpRef<BlueprintAbility> TargetAbility = new BpRef<BlueprintAbility>();

	[HideIf("OnSourceAbility")]
	public BpRef<BlueprintAbilityTag> TargetTag = new BpRef<BlueprintAbilityTag>();

	public RestrictionCalculator Restriction = new RestrictionCalculator();

	protected override void OnActivate()
	{
		if (OnSourceAbility)
		{
			BlueprintAbility targetAbility = base.Context.SourceAbility?.OriginalBlueprint ?? throw new InvalidOperationException();
			base.Owner.GetOrCreate<PartAbilityModifiers>().AddModifier(Modifier, targetAbility, base.Fact, this);
			return;
		}
		BlueprintAbility maybeBlueprint = TargetAbility.MaybeBlueprint;
		if (maybeBlueprint != null)
		{
			base.Owner.GetOrCreate<PartAbilityModifiers>().AddModifier(Modifier, maybeBlueprint, base.Fact, this);
			return;
		}
		BlueprintAbilityTag maybeBlueprint2 = TargetTag.MaybeBlueprint;
		if (maybeBlueprint2 != null)
		{
			base.Owner.GetOrCreate<PartAbilityModifiers>().AddModifier(Modifier, maybeBlueprint2, base.Fact, this);
		}
		else
		{
			base.Owner.GetOrCreate<PartAbilityModifiers>().AddModifier(Modifier, base.Fact, this);
		}
	}

	protected override void OnDeactivate()
	{
		base.Owner.GetOptional<PartAbilityModifiers>()?.RemoveModifier(base.Runtime);
	}

	public RestrictionCalculator GetRestriction()
	{
		return Restriction;
	}
}
