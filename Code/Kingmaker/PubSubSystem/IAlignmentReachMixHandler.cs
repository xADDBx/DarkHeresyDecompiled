using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Alignments;

namespace Kingmaker.PubSubSystem;

public interface IAlignmentReachMixHandler : ISubscriber<IBaseUnitEntity>, ISubscriber
{
	void HandleAlignmentReachedMix(AlignmentMix mix);
}
