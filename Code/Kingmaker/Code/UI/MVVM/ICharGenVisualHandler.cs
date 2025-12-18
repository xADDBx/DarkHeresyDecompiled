using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ICharGenVisualHandler : ISubscriber
{
	void HandleShowCloth(bool showCloth);
}
