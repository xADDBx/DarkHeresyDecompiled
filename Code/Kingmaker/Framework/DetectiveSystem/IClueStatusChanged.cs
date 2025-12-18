using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Framework.DetectiveSystem;

public interface IClueStatusChanged : ISubscriber
{
	void HandleClueStatusChanged(BlueprintClue blueprint);
}
