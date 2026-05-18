namespace Kingmaker.Code.UI.MVVM;

public class BrickSpaceVM : TooltipBrickVM
{
	public readonly float? Height;

	public BrickSpaceVM(float? height = null)
	{
		Height = height;
	}
}
