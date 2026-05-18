using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.Framework.PubSubSystem;

public interface ICameraMovementHandler : ISubscriber
{
	void HandleCameraRotated(float angle);

	void HandleCameraTransformed(float distance);
}
