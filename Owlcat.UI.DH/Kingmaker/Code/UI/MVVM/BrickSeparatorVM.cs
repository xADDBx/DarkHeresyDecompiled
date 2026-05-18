using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSeparatorVM : TooltipBrickVM
{
	public readonly TooltipBrickElementType Type;

	public BrickSeparatorVM(TooltipBrickElementType type = TooltipBrickElementType.Big)
	{
		Type = type;
	}
}
