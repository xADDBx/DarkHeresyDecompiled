using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickGlobalMapPosition : ITooltipBrick
{
	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickGlobalMapPositionVM();
	}
}
