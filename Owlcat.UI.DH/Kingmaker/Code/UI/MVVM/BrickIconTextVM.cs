namespace Kingmaker.Code.UI.MVVM;

public class BrickIconTextVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly bool IsShowIcon;

	public BrickIconTextVM(string text, bool isShowIcon = true)
	{
		Text = text;
		IsShowIcon = isShowIcon;
	}
}
