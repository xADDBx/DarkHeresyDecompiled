using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("20c1766e29e24c618baee1d9634ae045")]
public abstract class ChanceRollTrigger : MechanicEntityFactComponentDelegate
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList OnAnyResult;

	public ActionList OnSuccess;

	public ActionList OnFailure;

	protected void TryTrigger(RuleRollChance rule)
	{
		if (!Restrictions.IsPassed(base.Context, null, null, rule))
		{
			return;
		}
		using (SimpleContextData<TargetWrapper, MechanicsContext.Scope.Target>.SetIfNotNull(rule.Target))
		{
			OnAnyResult.Run();
			if (rule.Success)
			{
				OnSuccess.Run();
			}
			else
			{
				OnFailure.Run();
			}
		}
	}
}
