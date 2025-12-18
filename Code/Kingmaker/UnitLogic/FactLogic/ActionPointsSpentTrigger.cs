using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[TypeId("e36bc5ac422a452cb2757717cbd3f5a4")]
public class ActionPointsSpentTrigger : ActionPointsChangedTrigger, IUnitSpentActionPoints<EntitySubscriber>, IUnitSpentActionPoints, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitSpentActionPoints, EntitySubscriber>, IUnitSpentMovementPoints<EntitySubscriber>, IUnitSpentMovementPoints, IEventTag<IUnitSpentMovementPoints, EntitySubscriber>
{
	[SerializeField]
	private ContextValue m_TriggerValue;

	private int TriggerValue => m_TriggerValue.Calculate(base.Context);

	public void HandleUnitSpentActionPoints(int actionPointsSpent)
	{
		if (m_Type == PointsType.Yellow || Restriction.IsPassed(base.Context, base.Owner))
		{
			int actionPoints = base.Owner.CombatState.ActionPoints;
			if (actionPoints <= TriggerValue && (actionPoints + actionPointsSpent > TriggerValue || actionPointsSpent == -1))
			{
				base.Fact.RunActionInContext(Actions);
			}
		}
	}

	public void HandleUnitSpentMovementPoints(float movementPointsSpent)
	{
		if (m_Type == PointsType.Blue || Restriction.IsPassed(base.Context, base.Owner))
		{
			float movementPoints = base.Owner.CombatState.MovementPoints;
			if (movementPoints <= (float)TriggerValue && (movementPoints + movementPointsSpent > (float)TriggerValue || movementPointsSpent < 0f))
			{
				base.Fact.RunActionInContext(Actions);
			}
		}
	}
}
