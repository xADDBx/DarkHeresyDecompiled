using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTripleTextConsoleView : TooltipBrickTripleTextView, IConsoleTooltipBrick
{
	[Header("Console")]
	[SerializeField]
	private OwlcatMultiButton m_LeftMultiButton;

	[SerializeField]
	private OwlcatMultiButton m_MiddleMultiButton;

	[SerializeField]
	private OwlcatMultiButton m_RightMultiButton;

	private SimpleConsoleNavigationEntity m_LeftSimpleConsoleNavigationEntity;

	private SimpleConsoleNavigationEntity m_MiddleSimpleConsoleNavigationEntity;

	private SimpleConsoleNavigationEntity m_RightSimpleConsoleNavigationEntity;

	private GridConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	public MonoBehaviour MonoBehaviour => (MonoBehaviour)m_NavigationBehaviour.DeepestNestedFocus;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new GridConsoleNavigationBehaviour().AddTo(this);
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_LeftSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_LeftMultiButton);
		m_MiddleSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_MiddleMultiButton);
		m_RightSimpleConsoleNavigationEntity = new SimpleConsoleNavigationEntity(m_RightMultiButton);
		m_NavigationBehaviour.AddRow<SimpleConsoleNavigationEntity>(m_LeftSimpleConsoleNavigationEntity, m_MiddleSimpleConsoleNavigationEntity, m_RightSimpleConsoleNavigationEntity);
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
