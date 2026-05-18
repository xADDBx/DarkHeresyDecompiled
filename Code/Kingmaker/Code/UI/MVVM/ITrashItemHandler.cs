using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ITrashItemHandler : ISubscriber<IItemEntity>, ISubscriber
{
	void HandleItemAddedToTrash(ItemEntity item);

	void HandleItemRemovedFromTrash(ItemEntity item);
}
public interface ITrashItemHandler<TTag> : ITrashItemHandler, ISubscriber<IItemEntity>, ISubscriber, IEventTag<ITrashItemHandler, TTag>
{
}
