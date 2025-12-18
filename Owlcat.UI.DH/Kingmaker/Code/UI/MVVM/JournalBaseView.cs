using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class JournalBaseView : View<JournalVM>
{
	private bool m_IsShowed;

	public virtual void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		Show();
	}

	protected override void OnUnbind()
	{
		Hide();
	}

	private void Show()
	{
		if (!m_IsShowed)
		{
			m_IsShowed = true;
			base.gameObject.SetActive(value: true);
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: true, FullScreenUIType.Journal);
			});
		}
	}

	private void Hide()
	{
		if (m_IsShowed)
		{
			m_IsShowed = false;
			base.gameObject.SetActive(value: false);
			EventBus.RaiseEvent(delegate(IFullScreenUIHandler h)
			{
				h.HandleFullScreenUiChanged(state: false, FullScreenUIType.Journal);
			});
		}
	}
}
