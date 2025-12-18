using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickAbilityScoresBlockConsoleView : TooltipBrickAbilityScoresBlockView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_TitleFrame;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddEntityVertical(m_TitleFrame);
		m_NavigationBehaviour.AddEntityVertical((m_AbilityScoresBlockView as CharInfoAbilityScoresBlockConsoleView)?.GetNavigationBehaviour());
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
