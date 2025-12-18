using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[KDB("A global trigger. Reacts to RulePerformAbility.OnEventDidTrigger.\r\nCan filter events by range from fact owner, by caster/target faction and by overwatch area.\r\nCan run specified actions in context of either caster or target, but not fact owner.\r\n\t")]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Ability/AbilityUsedAroundTrigger")]
[TypeId("fc7d47b2586912942938f8a813b52e9b")]
public class AbilityUsedAroundTrigger : UnitFactComponentDelegate, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber
{
	[KDB("Caster must be from this faction")]
	public ParticipantFaction caster;

	[KDB("Target must be from this faction")]
	public ParticipantFaction target;

	[KDB("Selects context in which actions will be run (caster/target)")]
	public TargetSelection targetSelector;

	[KDB("Filter events by range from fact owner")]
	public bool limitByRange;

	[ShowIf("limitByRange")]
	public int minRange;

	[ShowIf("limitByRange")]
	public int maxRange;

	[KDB("Actions to run when event is triggered and all filters are passed")]
	public ActionList Actions;

	private void RunAction(AbilityData _, MechanicEntity initiator, TargetWrapper target)
	{
		base.Fact.RunActionInContext(Actions, SelectTarget(initiator, target).ToITargetWrapper());
	}

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public MechanicEntity SelectTarget(MechanicEntity initiator, TargetWrapper target)
	{
		if (targetSelector != 0)
		{
			return target.Entity;
		}
		return initiator;
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (!CheckFaction(evt.ConcreteInitiator, caster) || !CheckFaction(evt.AbilityTarget.Entity, target))
		{
			return;
		}
		bool flag = false;
		if (limitByRange && base.Context.MaybeOwner != null)
		{
			int customCellDistance = GeometryUtils.GetCustomCellDistance(base.Context.MaybeOwner.Position, SelectTarget(evt.ConcreteInitiator, evt.AbilityTarget).Position, GraphParamsMechanicsCache.GridCellSize);
			if (customCellDistance >= minRange || customCellDistance <= maxRange)
			{
				flag = true;
			}
		}
		if (!limitByRange || flag)
		{
			RunAction(evt.Ability, evt.ConcreteInitiator, evt.AbilityTarget);
		}
	}

	private bool CheckFaction(MechanicEntity entity, ParticipantFaction faction)
	{
		if (entity == null)
		{
			return false;
		}
		PartCombatGroup combatGroupOptional = entity.GetCombatGroupOptional();
		if (combatGroupOptional == null)
		{
			return false;
		}
		switch (faction)
		{
		case ParticipantFaction.Any:
			return true;
		case ParticipantFaction.Enemy:
			return combatGroupOptional.IsEnemy(base.Context.MaybeOwner);
		case ParticipantFaction.OtherAlly:
			if (entity != base.Context.MaybeOwner)
			{
				return combatGroupOptional.IsAlly(base.Context.MaybeOwner);
			}
			return false;
		case ParticipantFaction.AllyAndOwner:
			if (entity != base.Context.MaybeOwner)
			{
				return combatGroupOptional.IsAlly(base.Context.MaybeOwner);
			}
			return true;
		default:
			PFLog.Default.Error("WarhammerAbilityUsedAroundTrigger: unsupported ParticipantFaction type");
			return false;
		}
	}
}
