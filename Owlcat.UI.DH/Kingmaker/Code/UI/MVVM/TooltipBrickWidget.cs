using ObservableCollections;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWidget : ITooltipBrick
{
	private readonly TooltipBrickWidgetVM m_WidgetVM;

	public TooltipBrickWidget(ObservableList<ITooltipBrick> bricks, TooltipBrickText tooltipBrickText = null)
	{
		m_WidgetVM = new TooltipBrickWidgetVM(bricks, tooltipBrickText);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return m_WidgetVM;
	}
}
