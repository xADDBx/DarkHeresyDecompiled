using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IUnequipItemHandler : ISubscriber
{
	void HandleUnequipItem();
}
