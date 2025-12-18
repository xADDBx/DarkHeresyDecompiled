using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Framework.DetectiveSystem;

public interface ICaseStatusChanged : ISubscriber
{
	void HandleCaseStatusChanged(BlueprintCase blueprint);
}
