using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("5a67d0c6937040a6a5b86c774aa1d6c4")]
public abstract class ForceMoveTrigger : MechanicEntityFactComponentDelegate
{
	protected enum UnitForceMoveType
	{
		Push,
		Overpenetration
	}

	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	[SerializeField]
	protected UnitForceMoveType ForceMoveType;

	protected void TryTrigger(RulePerformAttack rule)
	{
		if (Restrictions.IsPassed(base.Context, null, null, rule) && rule.ResultDamageRule != null && (ForceMoveType != 0 || !rule.FromOverpenetration) && (ForceMoveType != UnitForceMoveType.Overpenetration || rule.FromOverpenetration))
		{
			OnTrigger(rule);
		}
	}

	protected abstract void OnTrigger(RulePerformAttack rule);
}
