using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSliderConsoleView : TooltipBrickSliderView, IConsoleTooltipBrick
{
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
		foreach (BrickSliderValueView sliderValueView in m_SliderValueViews)
		{
			if (sliderValueView is BrickSliderValueConsoleView brickSliderValueConsoleView)
			{
				m_NavigationBehaviour.AddEntityVertical(brickSliderValueConsoleView.GetConsoleEntity());
			}
		}
	}

	public IConsoleEntity GetConsoleEntity()
	{
		CreateNavigation();
		return m_NavigationBehaviour;
	}
}
