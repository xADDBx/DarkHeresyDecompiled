using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Root;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintAbility))]
[ComponentName("Caster Restriction/AbilityResourceBuff")]
[TypeId("ce26e27212da4e27bc9c90fc239153e0")]
public class AbilityResourceBuff : BlueprintComponent, IAbilityResourceLogic, IAbilityRestriction
{
	public ContextValue Cost;

	public BlueprintBuffReference Buff;

	public bool IsSpendResource()
	{
		return true;
	}

	public bool IsAbilityRestrictionPassed(AbilityData ability)
	{
		return CalculateCost(ability) <= CalculateResourceAmount(ability);
	}

	public string GetAbilityRestrictionUIText()
	{
		return LocalizedTexts.Instance.Reasons.NoResourcesBuff;
	}

	public void Spend(AbilityData ability)
	{
		int count = CalculateCost(ability);
		ability.Caster?.Buffs.GetBuff(Buff)?.RemoveRank(count);
	}

	public int CalculateCost(AbilityData ability)
	{
		return Cost.Calculate(ability.ClaimExecutionContext(ability.Caster));
	}

	public int CalculateResourceAmount(AbilityData ability)
	{
		return ability.Caster.Buffs.GetBuff(Buff)?.GetRank() ?? (-1);
	}
}
