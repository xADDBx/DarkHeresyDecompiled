using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class FirstLaunchSettingsPageBaseView<TViewModel> : View<TViewModel> where TViewModel : FirstLaunchSettingsPageVM
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	protected IConsoleEntity[] AdditionalEntities;

	public void SetNavigationBehaviour(GridConsoleNavigationBehaviour navigationBehaviour)
	{
		m_NavigationBehaviour = navigationBehaviour;
		SetNavigationBehaviourImpl(navigationBehaviour);
	}

	public void AddNavigationEntities(IConsoleEntity[] entities)
	{
		AdditionalEntities = entities;
	}

	protected virtual void SetNavigationBehaviourImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		BuildNavigation();
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_NavigationBehaviour.Clear();
	}

	public void ClearNavigationBehaviour()
	{
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_NavigationBehaviour.Clear();
	}

	private void BuildNavigation()
	{
		BuildNavigationImpl(m_NavigationBehaviour);
		BuildAdditionalNavigation(m_NavigationBehaviour);
	}

	protected virtual void BuildNavigationImpl(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}

	protected virtual void BuildAdditionalNavigation(GridConsoleNavigationBehaviour navigationBehaviour)
	{
	}
}
