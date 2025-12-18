using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickAbilityScoresConsoleView : TooltipBrickAbilityScoresView, IConsoleTooltipBrick, IMonoBehaviour
{
	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	public MonoBehaviour MonoBehaviour => (MonoBehaviour)m_NavigationBehaviour.DeepestNestedFocus;

	protected override void OnBind()
	{
		m_AbilityScoresBlockView.Bind(base.ViewModel.AbilityScoresBlock);
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddRow((m_AbilityScoresBlockView as CharInfoAbilityScoresBlockConsoleView)?.AbilityScores);
		return m_NavigationBehaviour;
	}
}
