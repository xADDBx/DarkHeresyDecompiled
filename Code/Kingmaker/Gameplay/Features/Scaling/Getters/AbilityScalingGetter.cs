using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Framework;
using Kingmaker.Code.Framework.Abilities.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Features.Scaling.Utility;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Scaling.Getters;

[Serializable]
[TypeId("6181abb5a1af4f679dde4022ccf1407a")]
public sealed class AbilityScalingGetter : IntPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase, PropertyContextAccessor.IOptionalMechanicContext, PropertyContextAccessor.IOptionalFact
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Ability Scaling";
	}

	protected override int GetBaseValue()
	{
		AbilityData ability;
		MechanicEntity caster;
		ScalingInfo? scaling = GetScaling(out ability, out caster);
		if (!scaling.HasValue)
		{
			return 0;
		}
		return scaling.Value.Calculator.GetValue(caster ?? base.CurrentEntity, null, null, null, ability);
	}

	private ScalingInfo? GetScaling([CanBeNull] out AbilityData ability, [CanBeNull] out MechanicEntity caster)
	{
		if (base.Owner is BlueprintToggleAbility)
		{
			MechanicEntityFact fact = this.GetFact();
			if (fact != null)
			{
				ability = null;
				caster = fact.Caster;
				return fact.GetScaling();
			}
		}
		SimpleBlueprint owner = base.Owner;
		if (owner is BlueprintAbility || owner is BlueprintAbilityModifier)
		{
			ability = this.GetAbility();
			if (ability != null)
			{
				caster = ability.Caster;
				return ability.GetScaling();
			}
		}
		if (this.GetFact() is ToggleAbility toggleAbility)
		{
			ability = null;
			caster = toggleAbility.Owner;
			return toggleAbility.GetScaling();
		}
		if (this.GetFact()?.SourceFact is ToggleAbility toggleAbility2)
		{
			ability = null;
			caster = toggleAbility2.Caster;
			return toggleAbility2.GetScaling();
		}
		AbilityData abilityData = this.GetEvalContext()?.SourceAbility;
		if ((object)abilityData != null)
		{
			ability = abilityData;
			caster = ability.Caster;
			return ability.GetScaling();
		}
		ability = null;
		caster = base.Context.CurrentEntity;
		BlueprintMechanicEntityFact blueprintMechanicEntityFact = base.Owner as BlueprintMechanicEntityFact;
		ScalingInfo? result = blueprintMechanicEntityFact?.GetScaling(caster);
		if (result.HasValue)
		{
			return result;
		}
		if (blueprintMechanicEntityFact != null)
		{
			return ScalingHelper.GetScalingFromDescriptionFact(blueprintMechanicEntityFact, caster);
		}
		return null;
	}
}
