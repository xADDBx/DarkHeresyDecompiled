using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Common;

namespace Kingmaker.Code.UI.MVVM;

public interface ISetInventorySorterHandler : ISubscriber
{
	void HandleSetInventorySorter(ItemsSorterType sorterType);
}
