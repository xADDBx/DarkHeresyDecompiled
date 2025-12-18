using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSeparatorVM : TooltipBaseBrickVM
{
	public readonly TooltipBrickElementType Type;

	public TooltipBrickSeparatorVM(TooltipBrickElementType type = TooltipBrickElementType.Big)
	{
		Type = type;
	}
}
