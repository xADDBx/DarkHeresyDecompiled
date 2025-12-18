using Kingmaker.Code.UI.MVVM;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IHoverActionBarSlotHandler : ISubscriber
{
	void HandlePointerEnterActionBarSlot(MechanicActionBarSlot ability);

	void HandlePointerExitActionBarSlot(MechanicActionBarSlot ability);

	void HandlePointerEnterAttackGroupAbilitySlot(MechanicActionBarSlot ability);

	void HandlePointerExitAttackGroupAbilitySlot(MechanicActionBarSlot ability);
}
