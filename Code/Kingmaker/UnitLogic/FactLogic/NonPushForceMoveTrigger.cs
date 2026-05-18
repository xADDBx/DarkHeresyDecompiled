using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[AllowedOn(typeof(BlueprintUnit))]
[ComponentName("Movement/NonPushForceMoveTrigger")]
[TypeId("20cd904e35bf478fbc8d294361266f39")]
public class NonPushForceMoveTrigger : UnitBuffComponentDelegate, IUnitAbilityNonPushForceMoveHandler, ISubscriber
{
	public ContextValue ValueMultiplier;

	public ContextValue ValueBonus;

	public ContextPropertyName ContextPropertyName;

	public bool OnlyFromOwner;

	public ActionList Actions;

	public void HandleUnitNonPushForceMove(int distanceInCells, IEvalContext context, UnitEntity movedTarget)
	{
		if (!OnlyFromOwner || context?.Caster == base.Owner)
		{
			EvalContext.Current[ContextPropertyName] = distanceInCells * ValueMultiplier.Calculate(context) + ValueBonus.Calculate(context);
			base.Fact.RunActionInContext(Actions, movedTarget);
		}
	}
}
