using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IEquipSlotHoverHandler : ISubscriber<ItemEntity>, ISubscriber
{
	void HandleHoverStart(ItemEntity item);

	void HandleHoverStop();
}
