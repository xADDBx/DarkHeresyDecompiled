using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;

namespace Kingmaker.RuleSystem.Rules;

[RuleRoles(Initiator = "attacker", Target = "defender")]
public class RulePerformBodyPartHitRoll : RulebookEvent
{
	public RuleRollD100 ResultD100 { get; private set; }

	public BlueprintBodyPart ResultHitLocation { get; private set; }

	public bool AlwaysHitDefaultBodyPart
	{
		get
		{
			AbilityData ability = base.Reason.Ability;
			if ((object)ability != null && ability.IsAoe)
			{
				if (!ability.IsRanged || (bool)ability.Caster.Features.HitRandomBodyPartWithAoeRanged)
				{
					if (ability.IsThrow)
					{
						return !ability.Caster.Features.HitRandomBodyPartWithAoeThrow;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public RulePerformBodyPartHitRoll([NotNull] MechanicEntity self)
		: base(self)
	{
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		AbilityData ability = base.Reason.Ability;
		if ((object)ability != null && ability.IsPrecise)
		{
			ResultHitLocation = ability.PreciseBodyPart ?? base.Self.DefaultBodyPart;
			return;
		}
		if (AlwaysHitDefaultBodyPart)
		{
			ResultHitLocation = base.Self.DefaultBodyPart;
			return;
		}
		ResultD100 = RulebookEvent.RollD100();
		int num = 0;
		foreach (BlueprintBodyPart bodyPart in base.Self.BodyParts)
		{
			if (bodyPart.CanBeHitRandomly)
			{
				num += bodyPart.HitChance;
				if ((int)ResultD100 <= num)
				{
					ResultHitLocation = bodyPart;
					return;
				}
			}
		}
		ResultHitLocation = base.Self.DefaultBodyPart;
	}
}
