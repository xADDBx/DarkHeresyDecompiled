using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IMoveItemHandler : ISubscriber
{
	void HandleMoveItem(bool isEquip);
}
