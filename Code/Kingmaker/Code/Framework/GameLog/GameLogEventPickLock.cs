using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventPickLock : GameLogEvent<GameLogEventPickLock>
{
	public enum ResultType
	{
		Success,
		Fail,
		CriticalFail
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IPickLockHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
	{
		public void HandlePickLockSuccess(MapObjectEntity mapObject)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, mapObject, ResultType.Success);
		}

		public void HandlePickLockFail(MapObjectEntity mapObject, bool critical)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, mapObject, (!critical) ? ResultType.Fail : ResultType.CriticalFail);
		}

		private void AddEvent(BaseUnitEntity unit, MapObjectEntity mapObject, ResultType result)
		{
			AddEvent(new GameLogEventPickLock(unit, mapObject, result));
		}
	}

	public readonly BaseUnitEntity Actor;

	public readonly MapObjectEntity MapObject;

	public readonly ResultType Result;

	private GameLogEventPickLock(BaseUnitEntity actor, MapObjectEntity mapObject, ResultType result)
	{
		Actor = actor;
		MapObject = mapObject;
		Result = result;
	}

	protected override bool TrySwallowEventInternal(GameLogEvent @event)
	{
		return base.TrySwallowEventInternal(@event);
	}
}
