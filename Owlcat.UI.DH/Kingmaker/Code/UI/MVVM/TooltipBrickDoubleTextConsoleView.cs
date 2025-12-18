using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickDoubleTextConsoleView : TooltipBrickDoubleTextView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_LeftMultiButton;

	[SerializeField]
	private OwlcatMultiButton m_RightMultiButton;

	private SimpleConsoleNavigationEntity m_LeftSimpleConsoleNavigationEntity;

	private SimpleConsoleNavigationEntity m_RightSimpleConsoleNavigationEntity;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	public MonoBehaviour MonoBehaviour => (MonoBehaviour)m_NavigationBehaviour.DeepestNestedFocus;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
	}

	public void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_LeftSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_LeftMultiButton);
		m_RightSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_RightMultiButton);
		m_NavigationBehaviour.AddRow<SimpleConsoleNavigationEntity>(m_LeftSimpleConsoleNavigationEntity, m_RightSimpleConsoleNavigationEntity);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
