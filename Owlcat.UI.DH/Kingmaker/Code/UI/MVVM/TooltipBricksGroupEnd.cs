using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBricksGroupEnd : ITooltipBrick
{
	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBricksGroupVM(TooltipBricksGroupType.End, hasBackground: true, null, null, null);
	}
}
