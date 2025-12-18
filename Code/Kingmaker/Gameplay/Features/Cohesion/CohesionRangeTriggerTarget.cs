using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Features.Cohesion;

[Serializable]
[ComponentName("Cohesion/CohesionRangeTriggerTarget")]
[TypeId("b40f1c6aebc24ae199d735a3372fb1e6")]
public sealed class CohesionRangeTriggerTarget : CohesionRangeTrigger, IEntityEnterCohesionRangeHandler, ISubscriber<IUnitEntity>, ISubscriber, IEntityExitCohesionRangeHandler, IEntityMoveInCohesionRangeHandler, IEntityStartTurnInCohesionRangeHandler, IEntityEndTurnInCohesionRangeHandler
{
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

	private new void TryTrigger(EventType eventType, MechanicEntity entity)
	{
		if (entity == base.Owner)
		{
			base.TryTrigger(eventType, (MechanicEntity)EventInvokerExtensions.UnitEntity);
		}
	}
}
