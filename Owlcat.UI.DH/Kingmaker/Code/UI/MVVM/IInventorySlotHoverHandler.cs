using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IInventorySlotHoverHandler : ISubscriber<ItemEntity>, ISubscriber
{
	void HandleHoverStart(ItemSlot slot);

	void HandleHoverStop();
}
