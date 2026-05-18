using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class StatCheckLootPageView<TViewModel> : View<TViewModel> where TViewModel : StatCheckLootPageVM
{
	protected bool m_IsVisible;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		InitializeImpl();
	}

	public void SetVisibility(bool visible)
	{
		if (visible != m_IsVisible)
		{
			m_IsVisible = visible;
			base.gameObject.SetActive(visible);
		}
	}

	protected virtual void InitializeImpl()
	{
	}
}
