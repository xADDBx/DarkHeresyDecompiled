using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[TypeId("81bc89747f864e3286c758a243d001dc")]
public class CheckAbilityAttackTypeGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalAbility, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	public AbilityAttackType Type;

	[ShowIf("IsSingle")]
	public BoolPropertyChecker.Mode IsPrecise;

	public bool IsSingle
	{
		get
		{
			AbilityAttackType type = Type;
			return type == AbilityAttackType.SingleMelee || type == AbilityAttackType.SingleRanged;
		}
	}

	protected override bool GetBaseValue()
	{
		AbilityAttackDelivery abilityAttackDelivery = this.GetAbility()?.Blueprint.GetComponent<AbilityAttackDelivery>();
		if (abilityAttackDelivery == null)
		{
			return false;
		}
		return Type switch
		{
			AbilityAttackType.SingleMelee => abilityAttackDelivery.IsSingle && abilityAttackDelivery.IsMelee && IsPrecise.Check(abilityAttackDelivery.IsPrecise), 
			AbilityAttackType.SingleRanged => abilityAttackDelivery.IsSingle && abilityAttackDelivery.IsRanged && IsPrecise.Check(abilityAttackDelivery.IsPrecise), 
			AbilityAttackType.BurstRanged => abilityAttackDelivery.IsBurst && abilityAttackDelivery.IsRanged, 
			AbilityAttackType.AoeMelee => abilityAttackDelivery.IsAoe && abilityAttackDelivery.IsMelee, 
			AbilityAttackType.AoeRanged => abilityAttackDelivery.IsAoe && abilityAttackDelivery.IsRanged, 
			AbilityAttackType.AoeThrow => abilityAttackDelivery.IsAoe && abilityAttackDelivery.IsThrow, 
			_ => false, 
		};
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Ability AttackType is {Type}" + ((IsSingle && IsPrecise == BoolPropertyChecker.Mode.Ignore) ? " and ignore Precise" : ((IsSingle && IsPrecise == BoolPropertyChecker.Mode.True) ? " and Precise" : ""));
	}
}
