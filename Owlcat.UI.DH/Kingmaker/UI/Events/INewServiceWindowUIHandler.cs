using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.UI.Events;

public interface INewServiceWindowUIHandler : ISubscriber
{
	void HandleCloseAll();

	void HandleOpenWindowOfType(ServiceWindowsType type);

	void HandleOpenEncyclopedia(INode page = null);

	void HandleOpenLocalMap();
}
