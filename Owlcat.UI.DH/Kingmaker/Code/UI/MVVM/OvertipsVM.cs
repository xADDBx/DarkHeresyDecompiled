using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class OvertipsVM : ViewModel, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	protected OvertipsVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	public abstract void HandleOnShowBark(string text);

	public abstract void HandleOnShowLinkedBark(string text, string encyclopediaLink);

	public abstract void HandleOnHideBark();
}
