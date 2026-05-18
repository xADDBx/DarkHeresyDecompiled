using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ServoSkullBarkVM : ViewModel, IBarkHandler, ISubscriber<IEntity>, ISubscriber
{
	private readonly ReactiveProperty<string> m_Bark = new ReactiveProperty<string>(null);

	public ReadOnlyReactiveProperty<string> Bark => m_Bark;

	public void HandleOnShowBark(string text)
	{
		m_Bark.Value = text;
	}

	public void HandleOnShowLinkedBark(string text, string encyclopediaLink)
	{
		m_Bark.Value = text;
	}

	public void HandleOnHideBark()
	{
		m_Bark.Value = null;
	}
}
