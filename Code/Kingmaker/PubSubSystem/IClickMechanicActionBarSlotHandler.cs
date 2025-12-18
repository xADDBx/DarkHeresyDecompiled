using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IClickMechanicActionBarSlotHandler : ISubscriber
{
	void HandleClickMechanicActionBarSlot(MechanicActionBarSlot ability);
}
