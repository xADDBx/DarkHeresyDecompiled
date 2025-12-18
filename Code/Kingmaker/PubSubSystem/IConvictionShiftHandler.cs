using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.PubSubSystem;

public interface IConvictionShiftHandler : ISubscriber
{
	void HandleAlignmentShift(IAlignmentShiftProvider provider);
}
