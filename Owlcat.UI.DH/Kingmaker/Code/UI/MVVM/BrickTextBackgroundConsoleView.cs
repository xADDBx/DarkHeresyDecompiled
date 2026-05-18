using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTextBackgroundConsoleView : BrickTextBackgroundView, IConsoleTooltipBrick
{
	public IConsoleEntity GetConsoleEntity()
	{
		return null;
	}
}
