using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Framework.DetectiveSystem;

public interface IConclusionStatusChanged : ISubscriber
{
	void HandleConclusionStatusChanged(BlueprintConclusion blueprint);
}
