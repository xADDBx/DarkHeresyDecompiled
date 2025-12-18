using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventCriticalEffectStageChanged : GameLogEvent<GameLogEventCriticalEffectStageChanged>
{
	private class EventsHandler : GameLogController.GameEventsHandler, ICriticalEffectStageChanged, ISubscriber<IMechanicEntity>, ISubscriber
	{
		void ICriticalEffectStageChanged.HandleCriticalEffectStageChanged(BlueprintBodyPart bodyPart, int previous, int current)
		{
			AddEvent(new GameLogEventCriticalEffectStageChanged(EventInvokerExtensions.MechanicEntity, bodyPart, previous, current));
		}
	}

	public readonly MechanicEntity Entity;

	public readonly BlueprintBodyPart BodyPart;

	public readonly int Previous;

	public readonly int Current;

	public GameLogEventCriticalEffectStageChanged(MechanicEntity entity, BlueprintBodyPart bodyPart, int previous, int current)
	{
		Entity = entity;
		BodyPart = bodyPart;
		Previous = previous;
		Current = current;
	}
}
