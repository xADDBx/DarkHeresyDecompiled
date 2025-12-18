using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Cohesion;

[Serializable]
[ComponentName("Cohesion/CohesionRangeTriggerOwner")]
[TypeId("3dcd604293324cc6bcb561493a92d3f8")]
public sealed class CohesionRangeTriggerOwner : CohesionRangeTrigger, IEntityEnterCohesionRangeHandler<EntitySubscriber>, IEntityEnterCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber, IEventTag<IEntityEnterCohesionRangeHandler, EntitySubscriber>, IEntityExitCohesionRangeHandler<EntitySubscriber>, IEntityExitCohesionRangeHandler, IEventTag<IEntityExitCohesionRangeHandler, EntitySubscriber>, IEntityMoveInCohesionRangeHandler<EntitySubscriber>, IEntityMoveInCohesionRangeHandler, IEventTag<IEntityMoveInCohesionRangeHandler, EntitySubscriber>, IEntityStartTurnInCohesionRangeHandler<EntitySubscriber>, IEntityStartTurnInCohesionRangeHandler, IEventTag<IEntityStartTurnInCohesionRangeHandler, EntitySubscriber>, IEntityEndTurnInCohesionRangeHandler<EntitySubscriber>, IEntityEndTurnInCohesionRangeHandler, IEventTag<IEntityEndTurnInCohesionRangeHandler, EntitySubscriber>
{
	public bool TriggerOnEnterWhenActivated = true;

	protected override void OnActivate()
	{
		if (!TriggerOnEnterWhenActivated)
		{
			return;
		}
		PartCohesion optional = base.Owner.GetOptional<PartCohesion>();
		if (optional == null)
		{
			return;
		}
		foreach (UnitEntity item in optional.UnitsInRange)
		{
			TryTrigger(EventType.Enter, item);
		}
	}

	void IEntityEnterCohesionRangeHandler.HandleEntityEnterCohesionRange(MechanicEntity entity)
	{
		TryTrigger(EventType.Enter, entity);
	}

	void IEntityExitCohesionRangeHandler.HandleEntityExitCohesionRange(MechanicEntity entity)
	{
		TryTrigger(EventType.Exit, entity);
	}

	void IEntityMoveInCohesionRangeHandler.HandleEntityMoveInCohesionRange(MechanicEntity entity)
	{
		TryTrigger(EventType.Move, entity);
	}

	void IEntityStartTurnInCohesionRangeHandler.HandleEntityStartTurnInCohesionRange(MechanicEntity entity)
	{
		TryTrigger(EventType.StartTurn, entity);
	}

	void IEntityEndTurnInCohesionRangeHandler.HandleEntityEndTurnInCohesionRange(MechanicEntity entity)
	{
		TryTrigger(EventType.EndTurn, entity);
	}
}
