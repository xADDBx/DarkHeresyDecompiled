using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class StatCheckLootPageView<TViewModel> : View<TViewModel> where TViewModel : StatCheckLootPageVM
{
	protected GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected InputLayer m_InputLayer;

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
			if (visible)
			{
				GamePad.Instance.PushLayer(m_InputLayer);
			}
			else
			{
				GamePad.Instance.PopLayer(m_InputLayer);
			}
		}
	}

	protected override void OnBind()
	{
		BuildNavigation();
	}

	protected override void OnUnbind()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour = null;
	}

	protected virtual void InitializeImpl()
	{
	}

	private void BuildNavigation()
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
		BuildNavigationImpl();
	}

	protected virtual void BuildNavigationImpl()
	{
	}
}
