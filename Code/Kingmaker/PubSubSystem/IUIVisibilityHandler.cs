using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.PubSubSystem;

public interface IUIVisibilityHandler : ISubscriber
{
	void HandleUIVisibilityChange(UIVisibilityFlags flags);
}
