using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;

namespace Kingmaker.Framework.Mechanics.Actor;

public readonly struct StatContext
{
	public readonly MechanicActor? Owner;

	public readonly MechanicActor? Against;

	public readonly AbilityData? Ability;

	public readonly MechanicEntityFact? Fact;

	public readonly DamageType? DamageType;

	public readonly BlueprintBodyPart? BodyPart;

	public readonly RulebookEvent? Rule;

	public bool HasProperties
	{
		get
		{
			if (!DamageType.HasValue && BodyPart == null)
			{
				return Rule != null;
			}
			return true;
		}
	}

	public StatContext(MechanicActor? owner = null, MechanicActor? against = null, AbilityData? ability = null, MechanicEntityFact? fact = null, DamageType? damageType = null, BlueprintBodyPart? bodyPart = null, RulebookEvent? rule = null)
	{
		Owner = owner;
		Against = against;
		Ability = ability;
		Fact = fact;
		DamageType = damageType;
		BodyPart = bodyPart;
		Rule = rule;
	}

	private StatContext(StatContext other, MechanicActor? owner = null, MechanicActor? against = null, AbilityData? ability = null, MechanicEntityFact? fact = null)
	{
		Owner = owner ?? other.Owner;
		Against = against ?? other.Against;
		Ability = ability ?? other.Ability;
		Fact = fact ?? other.Fact;
		DamageType = other.DamageType;
		BodyPart = other.BodyPart;
		Rule = other.Rule;
	}

	public StatContext WithOwner(MechanicActor owner)
	{
		return new StatContext(this, owner);
	}
}
