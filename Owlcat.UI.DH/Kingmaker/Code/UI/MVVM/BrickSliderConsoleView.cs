using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSliderConsoleView : BrickSliderView, IConsoleTooltipBrick
{
	public IConsoleEntity GetConsoleEntity()
	{
		return null;
	}
}
