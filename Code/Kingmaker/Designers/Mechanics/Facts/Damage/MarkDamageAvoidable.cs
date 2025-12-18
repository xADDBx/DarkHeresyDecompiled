using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Serializable]
[TypeId("e6e71e8f4a8e4c3fa868789c464cba6f")]
public abstract class MarkDamageAvoidable : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	protected void TryApply(RuleCalculateDamage rule)
	{
		if (Restrictions.IsPassed(base.Context, null, null, rule))
		{
			rule.Avoidable.Add(base.Fact);
		}
	}
}
