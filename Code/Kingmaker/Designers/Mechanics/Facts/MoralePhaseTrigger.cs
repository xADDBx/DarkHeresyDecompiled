using Kingmaker.Blueprints.Attributes;
using Kingmaker.Code.Gameplay.Parts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("f34fbc2e0e514b349eb373c92e3e82a7")]
public abstract class MoralePhaseTrigger : UnitFactComponentDelegate
{
	public MoralePhaseType TriggersOnState;

	public RestrictionCalculator Restrictions;

	public ActionList Actions;

	protected void TryTrigger(MoralePhaseType phase, MechanicEntity target)
	{
		if (TriggersOnState == phase && Restrictions.IsPassed(base.Context, base.Owner))
		{
			Actions.Run();
		}
	}
}
