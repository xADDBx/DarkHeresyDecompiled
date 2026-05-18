using ObservableCollections;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickWidgetVM : TooltipBrickVM
{
	public BrickTextVM TextBrickVM;

	public readonly ObservableList<ITooltipBrick> Bricks;

	public BrickWidgetVM(ObservableList<ITooltipBrick> bricks, BrickTextVM brickText = null)
	{
		Bricks = bricks;
		TextBrickVM = brickText;
	}
}
