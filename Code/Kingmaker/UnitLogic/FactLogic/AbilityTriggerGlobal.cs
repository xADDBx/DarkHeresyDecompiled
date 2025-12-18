using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("f24c2d6da8b04755b2b1a5ebbee085ae")]
public class AbilityTriggerGlobal : AbilityTrigger, IGlobalRulebookHandler<RulePerformAbility>, IRulebookHandler<RulePerformAbility>, ISubscriber, IGlobalRulebookSubscriber
{
	public bool AssignOwnerAsTarget;

	public bool AssignCasterAsTarget;

	public void OnEventAboutToTrigger(RulePerformAbility evt)
	{
	}

	public void OnEventDidTrigger(RulePerformAbility evt)
	{
		if (Restrictions.IsPassed(base.Context, null, null, evt))
		{
			BlueprintAbilityWrapper abilityWrapper = evt.Ability.Blueprint;
			MechanicEntity concreteInitiator = evt.ConcreteInitiator;
			TargetWrapper abilityTarget = evt.AbilityTarget;
			if ((concreteInitiator ?? abilityTarget.Entity) == null)
			{
				PFLog.Default.Error("AbilityTrigger: Both initiator and target are null!");
			}
			else if ((!ForOneAbility || abilityWrapper.SameAbility(base.Ability)) && (!ForMultipleAbilities || Abilities.HasItem((BlueprintAbilityReference r) => abilityWrapper.SameAbility(r))) && (!ForAbilityGroup || abilityWrapper.AbilityGroups.Contains(base.AbilityGroup)))
			{
				base.Fact.RunActionInContext(Action, AssignOwnerAsTarget ? ((TargetWrapper)base.Owner) : (AssignCasterAsTarget ? ((TargetWrapper)concreteInitiator) : abilityTarget));
			}
		}
	}
}
