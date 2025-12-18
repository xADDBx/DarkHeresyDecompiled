using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickIconPatternConsoleView : TooltipBrickIconPatternView, IConsoleTooltipBrick
{
	[SerializeField]
	private OwlcatMultiButton m_FrameButton;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonFirstFocus;

	[SerializeField]
	private OwlcatMultiButton m_MultiButtonSecondFocus;

	[SerializeField]
	private FloatConsoleNavigationBehaviour.NavigationParameters m_NavigationParameters;

	private FloatConsoleNavigationBehaviour m_NavigationBehaviour;

	public bool IsBinded => base.ViewModel != null;

	protected override void OnBind()
	{
		base.OnBind();
		m_NavigationBehaviour = new FloatConsoleNavigationBehaviour(m_NavigationParameters).AddTo(this);
	}

	private void CreateNavigation()
	{
		m_NavigationBehaviour.Clear();
		m_NavigationBehaviour.AddEntities<SimpleConsoleNavigationEntity>(new SimpleConsoleNavigationEntity(m_FrameButton, base.ViewModel.Tooltip));
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
