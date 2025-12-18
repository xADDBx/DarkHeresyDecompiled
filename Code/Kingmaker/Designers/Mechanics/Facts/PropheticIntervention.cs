using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("fd21b7be2c9a41b889835f5cf45fa909")]
public class PropheticIntervention : UnitFactComponentDelegate, IGlobalRulebookHandler<RuleDealDamage>, IRulebookHandler<RuleDealDamage>, ISubscriber, IGlobalRulebookSubscriber, ITurnBasedModeHandler, IInterruptTurnEndHandler, ISubscriber<IMechanicEntity>
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private BlueprintBuffReference m_Buff;

	public BlueprintBuff Buff => m_Buff?.Get();

	void IRulebookHandler<RuleDealDamage>.OnEventAboutToTrigger(RuleDealDamage rule)
	{
	}

	public void OnEventDidTrigger(RuleDealDamage evt)
	{
		if (evt.Target is UnitEntity unitEntity && evt.Initiator is UnitEntity unitEntity2 && evt.HPBeforeDamage > 0 && m_Restrictions.IsPassed(base.Context, null, null, evt))
		{
			PartHealth targetHealth = evt.TargetHealth;
			if ((targetHealth == null || targetHealth.HitPointsLeft <= 0) && unitEntity.IsAlly(base.Owner) && unitEntity.IsInCompanionRoster())
			{
				base.Owner.Parts.GetOrCreate<UnitPartPropheticIntervention>().NewEntry(unitEntity, evt.HPBeforeDamage, base.Owner.Position.GetNearestNodeXZUnwalkable());
				BuffDuration duration = new BuffDuration(null, BuffEndCondition.CombatEnd);
				unitEntity2.Buffs.Add(Buff, base.Context, duration);
			}
		}
	}

	public void HandleTurnBasedModeSwitched(bool isTurnBased)
	{
		if (!isTurnBased)
		{
			base.Owner.Parts.GetOptional<UnitPartPropheticIntervention>()?.Entries.Clear();
		}
	}

	public void HandleUnitEndInterruptTurn()
	{
		if (EventInvokerExtensions.MechanicEntity == base.Owner)
		{
			base.Owner.Parts.GetOptional<UnitPartPropheticIntervention>()?.Entries.Clear();
		}
	}
}
