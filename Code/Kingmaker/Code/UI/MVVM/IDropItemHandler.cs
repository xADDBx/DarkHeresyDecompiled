using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IDropItemHandler : ISubscriber
{
	void HandleDropItem(ItemEntity item, bool isSplit);
}
