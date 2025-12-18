using System.Linq;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathsListsConsoleView : CareerPathsListsCommonView
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	private readonly ReactiveProperty<bool> m_CanConfirm = new ReactiveProperty<bool>();

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_NavigationBehaviour?.Clear();
		m_NavigationBehaviour = null;
	}

	protected override void RefreshView()
	{
		base.RefreshView();
		UpdateNavigation();
	}

	private void CreateNavigation(IConsoleNavigationOwner owner)
	{
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour(owner);
		m_NavigationBehaviour.DeepestFocusAsObservable.Subscribe(OnFocusChanged).AddTo(this);
		UpdateNavigation();
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	private void UpdateNavigation()
	{
		if (m_NavigationBehaviour != null)
		{
			bool isFocused = m_NavigationBehaviour.IsFocused;
			m_NavigationBehaviour.Clear();
			m_NavigationBehaviour.AddColumn((from i in m_CareerPathsLists
				where i.ViewModel != null
				select i.GetNavigationBehaviour()).ToList());
			if (isFocused)
			{
				m_NavigationBehaviour.FocusOnCurrentEntity();
			}
		}
	}

	public GridConsoleNavigationBehaviour GetNavigationBehaviour(IConsoleNavigationOwner owner)
	{
		if (m_NavigationBehaviour == null)
		{
			CreateNavigation(owner);
		}
		foreach (CareerPathsListCommonView careerPathsList in m_CareerPathsLists)
		{
			if (careerPathsList.ViewModel != null)
			{
				careerPathsList.UpdateCurrentEntity();
			}
		}
		return m_NavigationBehaviour;
	}

	private void OnFocusChanged(IConsoleEntity entity)
	{
		m_CanConfirm.Value = (entity as IConfirmClickHandler)?.CanConfirmClick() ?? false;
	}

	private void OnConfirmClick()
	{
		m_NavigationBehaviour.OnConfirmClick();
		m_NavigationBehaviour.UnFocusCurrentEntity();
		m_NavigationBehaviour.FocusOnCurrentEntity();
	}

	public void AddInput(ref InputLayer inputLayer, ref ConsoleHintsWidget hintsWidget)
	{
	}
}
