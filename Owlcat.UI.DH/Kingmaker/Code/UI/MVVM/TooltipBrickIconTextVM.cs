using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickIconTextVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly bool IsShowIcon;

	public TooltipBrickIconTextVM(string text, bool isShowIcon = true)
	{
		Text = text;
		IsShowIcon = isShowIcon;
	}
}
