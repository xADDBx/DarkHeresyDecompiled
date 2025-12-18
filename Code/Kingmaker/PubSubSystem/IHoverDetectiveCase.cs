using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IHoverDetectiveCase : ISubscriber
{
	void DetectiveCaseHovered(BlueprintCase blueprintCase);

	void DetectiveCaseUnhovered(BlueprintCase blueprintCase);
}
