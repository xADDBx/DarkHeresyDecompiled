using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SubtitleVM : ViewModel, ISubtitleBarkHandler, ISubscriber
{
	private readonly ReactiveProperty<string> m_BarkText = new ReactiveProperty<string>(string.Empty);

	public ReadOnlyReactiveProperty<string> BarkText => m_BarkText;

	public SubtitleVM()
	{
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnDispose()
	{
		m_BarkText.Value = string.Empty;
		base.OnDispose();
	}

	public void HandleOnShowBark(string text)
	{
		m_BarkText.Value = text;
	}

	public void HandleOnHideBark()
	{
		m_BarkText.Value = string.Empty;
	}
}
