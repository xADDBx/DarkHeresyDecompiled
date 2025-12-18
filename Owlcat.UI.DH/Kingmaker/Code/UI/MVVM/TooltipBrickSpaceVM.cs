using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSpaceVM : TooltipBaseBrickVM
{
	public readonly float? Height;

	public TooltipBrickSpaceVM(float? height)
	{
		Height = height;
	}
}
