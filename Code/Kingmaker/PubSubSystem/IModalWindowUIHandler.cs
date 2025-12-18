using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IModalWindowUIHandler : ISubscriber
{
	void HandleModalWindowUiChanged(bool state, ModalWindowUIType modalWindowUIType);
}
