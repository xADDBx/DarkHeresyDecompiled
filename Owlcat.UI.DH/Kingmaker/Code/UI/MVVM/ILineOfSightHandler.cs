using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ILineOfSightHandler : ISubscriber
{
	void OnLineOfSightCreated(LineOfSightVM los);

	void OnLineOfSightDestroyed(LineOfSightVM los);
}
