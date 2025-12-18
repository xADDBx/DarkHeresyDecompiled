using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Disperse;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.Pool;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("80374c3de21440d1895ed480b8198f0e")]
public class DisperseDamage : UnitFactComponentDelegate, ITargetRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, ITargetRulebookSubscriber
{
	private sealed class ComponentData : IEntityFactComponentTransientData
	{
		public RuleDealDamage Rule;

		public MechanicEntity[] Targets;
	}

	public RestrictionCalculator OtherUnitApplyRestriction = new RestrictionCalculator();

	public ContextValueModifierWithType DamageModifyBeforeDispersing = new ContextValueModifierWithType();

	public bool ExcludeOwner;

	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage evt)
	{
		if (evt.Reason.Context == base.Context || evt.IsDispersedDamage)
		{
			return;
		}
		HashSet<MechanicEntity> value;
		using (CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Get(out value))
		{
			foreach (BaseUnitEntity allBaseAwakeUnit in Game.Instance.EntityPools.AllBaseAwakeUnits)
			{
				if ((!ExcludeOwner || allBaseAwakeUnit != base.Owner) && !allBaseAwakeUnit.IsDeadOrUnconscious && OtherUnitApplyRestriction.IsPassed(base.Context, currentTarget: allBaseAwakeUnit, currentEntity: base.Owner))
				{
					value.Add(allBaseAwakeUnit);
				}
			}
			if (value.Count >= 1)
			{
				evt.CancelDamage = true;
				evt.DisableGameLog = true;
				ComponentData componentData = RequestTransientData<ComponentData>();
				componentData.Rule = evt;
				componentData.Targets = value.ToArray();
			}
		}
	}

	void IRulebookHandler<RuleDealDamage>.OnEventDidTrigger(RuleDealDamage evt)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.Rule == evt)
		{
			MechanicEntity[] targets = componentData.Targets;
			componentData.Rule = null;
			componentData.Targets = null;
			RuleDisperseDamage rule = new RuleDisperseDamage(evt.Initiator, evt.Target, targets, evt.ResultDamage, base.Fact, DamageModifyBeforeDispersing);
			base.Context.TriggerRule(rule);
		}
	}
}
