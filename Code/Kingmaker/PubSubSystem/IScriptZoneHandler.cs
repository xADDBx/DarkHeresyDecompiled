using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IScriptZoneHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void OnUnitEnteredScriptZone(ScriptZoneEntity zone);

	void OnUnitExitedScriptZone(ScriptZoneEntity zone);
}
