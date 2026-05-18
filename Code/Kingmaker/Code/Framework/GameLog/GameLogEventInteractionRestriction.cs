using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.Code.Framework.GameLog;

public class GameLogEventInteractionRestriction : GameLogEvent<GameLogEventInteractionRestriction>
{
	public enum ResultType
	{
		MissingSkill,
		Jammed,
		CantDisarm
	}

	private class EventsHandler : GameLogController.GameEventsHandler, IInteractionRestrictionHandler, ISubscriber<IBaseUnitEntity>, ISubscriber
	{
		public void HandleMissingInteractionSkill(MapObjectEntity mapObject, StatType skill)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, mapObject, skill, ResultType.MissingSkill);
		}

		public void HandleJammed(MapObjectEntity mapObject)
		{
			AddEvent(null, mapObject, StatType.Unknown, ResultType.MissingSkill);
		}

		public void HandleCantDisarmTrap(TrapObjectData trap)
		{
			AddEvent(EventInvokerExtensions.BaseUnitEntity, trap, StatType.Unknown, ResultType.MissingSkill);
		}

		private void AddEvent(BaseUnitEntity actor, MapObjectEntity mapObject, StatType skill, ResultType result)
		{
			AddEvent(new GameLogEventInteractionRestriction(actor, mapObject, skill, result));
		}
	}

	public readonly BaseUnitEntity Actor;

	public readonly MapObjectEntity MapObject;

	public readonly StatType Skill;

	public readonly ResultType Result;

	public TrapObjectData TrapObject => MapObject as TrapObjectData;

	public GameLogEventInteractionRestriction(BaseUnitEntity actor, MapObjectEntity mapObject, StatType skill, ResultType result)
	{
		Actor = actor;
		MapObject = mapObject;
		Skill = skill;
		Result = result;
	}
}
