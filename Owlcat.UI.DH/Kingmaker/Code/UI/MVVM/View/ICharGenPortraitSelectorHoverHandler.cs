using Kingmaker.Blueprints;
using Kingmaker.PubSubSystem.Core.Interfaces;

namespace Kingmaker.Code.UI.MVVM.View;

public interface ICharGenPortraitSelectorHoverHandler : ISubscriber
{
	void HandleHoverStart(PortraitData portrait);

	void HandleHoverStop();
}
