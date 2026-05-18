using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View.MapObjects.Traps;

namespace Kingmaker.PubSubSystem;

public interface IInteractionRestrictionHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleMissingInteractionSkill(MapObjectEntity mapObject, StatType skill);

	void HandleJammed(MapObjectEntity mapObject);

	void HandleCantDisarmTrap(TrapObjectData trap);
}
