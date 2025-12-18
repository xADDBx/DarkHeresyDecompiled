using Kingmaker.Framework.Pathfinding;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IGridObstacleEnabledHandler : ISubscriber
{
	void HandleGridObstacleEnabled(GridObstacle gridObstacle, bool enabled);
}
