using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface IInfoWindowHandler : ISubscriber
{
	void HandleCloseTooltipInfoWindow();
}
