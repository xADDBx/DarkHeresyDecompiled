using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.Framework.Mechanics;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("50efb6f282344302b488deaee3b0ed8b")]
public abstract class CriticalEffectsTrigger : MechanicEntityFactComponentDelegate
{
	public enum AmountFilterType
	{
		Any,
		Zero,
		NotZero
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public AmountFilterType AmountFilter;

	public ActionList Actions = new ActionList();

	protected void TryApply(RulePerformCriticalEffects rule)
	{
		if (IsSuitable(rule))
		{
			Actions.RunWithTarget(rule.Target);
		}
	}

	private bool IsSuitable(RulePerformCriticalEffects rule)
	{
		if (AmountFilter switch
		{
			AmountFilterType.Any => true, 
			AmountFilterType.Zero => rule.ResultAmount == 0, 
			AmountFilterType.NotZero => rule.ResultAmount != 0, 
			_ => throw new ArgumentOutOfRangeException(), 
		})
		{
			return Restrictions.IsPassed(base.Context, base.Owner, null, rule);
		}
		return false;
	}
}
