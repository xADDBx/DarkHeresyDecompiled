using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintUnitFact))]
[ComponentName("Movement/MovedInCombatTrigger")]
[TypeId("12c0b652aac8a2a4abf467858236cd75")]
public class MovedInCombatTrigger : UnitFactComponentDelegate, IUnitCommandEndHandler, ISubscriber<IMechanicEntity>, ISubscriber
{
	public ActionList Actions;

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		if (command is UnitMoveToProper unitMoveToProper && base.Context.MaybeOwner == command.Executor)
		{
			EvalContext.Current[ContextPropertyName.Value1] = (int)unitMoveToProper.MovePointsSpent;
			base.Fact.RunActionInContext(Actions, base.Context.MaybeOwner.ToITargetWrapper());
		}
	}
}
