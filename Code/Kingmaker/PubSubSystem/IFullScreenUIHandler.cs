using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IFullScreenUIHandler : ISubscriber
{
	void HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType);
}
