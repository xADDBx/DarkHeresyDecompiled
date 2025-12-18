using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Framework.DetectiveSystem;

public interface IClueAddendumStatusChanged : ISubscriber
{
	void HandleClueAddendumStatusChanged(BlueprintClueAddendum blueprint);
}
