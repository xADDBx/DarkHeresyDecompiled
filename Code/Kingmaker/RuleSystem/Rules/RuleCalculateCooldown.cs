using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.ContextContract;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.FactLogic;

namespace Kingmaker.RuleSystem.Rules;

[RuleRoles(Initiator = "ability caster", Target = "caster (self)")]
public class RuleCalculateCooldown : RulebookEvent
{
	public List<GroupCooldownData> GroupCooldownsData = new List<GroupCooldownData>();

	public Cooldown CooldownComponent { get; private set; }

	public AbilityData Ability { get; }

	public int Result { get; set; }

	public RuleCalculateCooldown([NotNull] MechanicEntity initiator, AbilityData ability)
		: base(initiator)
	{
		Ability = ability;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		CooldownComponent = Ability.Blueprint.GetComponent<Cooldown>();
		Result = Ability.Blueprint.CooldownRounds + (CooldownComponent?.CooldownInRounds ?? 0);
		foreach (BlueprintAbilityGroup abilityGroup in Ability.AbilityGroups)
		{
			GroupCooldownData item = new GroupCooldownData
			{
				Group = abilityGroup,
				Cooldown = Rulebook.Trigger(new RuleCalculateGroupCooldown(base.Initiator, abilityGroup, this)).Result
			};
			GroupCooldownsData.Add(item);
		}
	}
}
