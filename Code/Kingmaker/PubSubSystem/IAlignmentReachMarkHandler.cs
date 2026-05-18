using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.PubSubSystem;

public interface IAlignmentReachMarkHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAlignmentMarkShift(AlignmentAxis axis, int previousMark, int reachedMark);
}
