using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUnitAbilityNonPushForceMoveHandler : ISubscriber
{
	void HandleUnitNonPushForceMove(int distanceInCells, IEvalContext context, UnitEntity movedTarget);
}
