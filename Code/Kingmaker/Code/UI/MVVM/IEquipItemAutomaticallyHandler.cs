using Kingmaker.Items;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface IEquipItemAutomaticallyHandler : ISubscriber
{
	void HandleEquipItemAutomatically(ItemEntity item);
}
