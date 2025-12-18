using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.PubSubSystem;

public interface IAlignmentRankShiftHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAlignmentRankShift(AlignmentShift shift);
}
