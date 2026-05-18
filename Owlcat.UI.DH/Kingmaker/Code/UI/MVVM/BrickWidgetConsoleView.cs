using Kingmaker.Code.UI.MVVM.View;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickWidgetConsoleView : BrickWidgetView, IConsoleTooltipBrick
{
	public IConsoleEntity GetConsoleEntity()
	{
		return null;
	}
}
