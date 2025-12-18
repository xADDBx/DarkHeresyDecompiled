using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[ComponentName("Movement/StepOnUnitTrigger")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("78416892d808478298265d953095c2ae")]
public class StepOnUnitTrigger : UnitFactComponentDelegate, IUnitMovementHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public ActionList MoveThroughUnitActions;

	public void HandleWaypointUpdate()
	{
		BaseUnitEntity baseUnitEntity = ContextData<EventInvoker>.Current?.InvokerEntity as BaseUnitEntity;
		if (base.Fact.Owner != baseUnitEntity)
		{
			return;
		}
		foreach (GridNodeBase occupiedNode in baseUnitEntity.GetOccupiedNodes())
		{
			if (!occupiedNode.TryGetFirstUnit(out var unit) || baseUnitEntity == unit)
			{
				continue;
			}
			using (base.Context.SetScope(unit.ToITargetWrapper()))
			{
				if (Restrictions.IsPassed(base.Context, base.Owner))
				{
					MoveThroughUnitActions?.Run();
				}
			}
		}
	}

	public void HandleMovementComplete()
	{
	}
}
