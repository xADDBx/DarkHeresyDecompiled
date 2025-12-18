using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFullScreenUIHandlerWorkaround : ISubscriber
{
	void HandleFullScreenUiChangedWorkaround(bool state, FullScreenUIType fullScreenUIType);
}
