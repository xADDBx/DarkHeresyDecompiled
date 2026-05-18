using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IClueStudyHandler : ISubscriber
{
	void HandleClueStudied();
}
