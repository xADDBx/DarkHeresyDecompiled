using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM;

public interface ISetCharInfoUnitPanelState : ISubscriber
{
	void SetUnitPanelNavigationState(bool state);
}
