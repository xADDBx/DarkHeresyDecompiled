using Kingmaker.Controllers.Clicks;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipsVM : ViewModel, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	public abstract void HandleOnShowBark(string text);

	public abstract void HandleOnShowLinkedBark(string text, string encyclopediaLink);

	public abstract void HandleOnHideBark();

	public void Navigate(int indexOffset)
	{
		Game.Instance.GetController<InteractionNavigationController>().Navigate(indexOffset);
	}

	public void Interact()
	{
		Entity focus = Game.Instance.GetController<InteractionNavigationController>().Focus;
		if (focus != null)
		{
			Game.Instance.GetController<PointerController>()?.SimulateClick(focus.View.GO, muteEvents: false);
		}
	}
}
