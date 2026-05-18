using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventDisarmTrap : GameLogEvent<GameLogEventDisarmTrap>
{
	public enum ResultType
	{
		Success,
		Fail,
		CriticalFail
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IDisarmTrapHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
	{
		public void HandleDisarmTrapSuccess(TrapObjectData trap)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, trap, ResultType.Success);
		}

		public void HandleDisarmTrapFail(TrapObjectData trap)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, trap, ResultType.Fail);
		}

		public void HandleDisarmTrapCriticalFail(TrapObjectData trap)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, trap, ResultType.CriticalFail);
		}

		private void AddEvent(BaseUnitEntity unit, TrapObjectData trap, ResultType result)
		{
			AddEvent(new GameLogEventDisarmTrap(unit, trap, result));
		}
	}

	public readonly BaseUnitEntity Actor;

	public readonly TrapObjectData Trap;

	public readonly ResultType Result;

	private GameLogEventDisarmTrap(BaseUnitEntity actor, TrapObjectData trap, ResultType result)
	{
		Actor = actor;
		Trap = trap;
		Result = result;
	}

	protected override bool TrySwallowEventInternal(GameLogEvent @event)
	{
		return base.TrySwallowEventInternal(@event);
	}
}
