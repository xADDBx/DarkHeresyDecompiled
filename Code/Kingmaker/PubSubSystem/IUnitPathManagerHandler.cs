using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Pathfinding;

namespace Kingmaker.PubSubSystem;

public interface IUnitPathManagerHandler : ISubscriber<IMechanicEntity>, ISubscriber
{
	void HandlePathAdded(Path path, float cost, List<BaseUnitEntity> enemiesAoO, bool hasThreats);

	void HandlePathRemoved();

	void HandleCurrentNodeChanged(float cost);
}
