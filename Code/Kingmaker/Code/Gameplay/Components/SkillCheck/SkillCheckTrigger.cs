using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components.SkillCheck;

[Serializable]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("ff38bc9aee934370a50e46dfcfe48539")]
public abstract class SkillCheckTrigger : MechanicEntityFactComponentDelegate
{
	public SkillCheckRestrictionCalculator Restrictions = new SkillCheckRestrictionCalculator();

	public ActionList Actions = new ActionList();

	protected void TryTrigger(RulePerformSkillCheck rule)
	{
		if (Restrictions.IsPassed(base.Context, null, null, rule))
		{
			Actions.Run();
		}
	}
}
