using Kingmaker.Framework.Pathfinding;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGridObstacleAwakeHandler : ISubscriber
{
	void HandleGridObstacleAwake(GridObstacle gridObstacle);
}
