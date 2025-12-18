using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickHintVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public TooltipBrickHintVM(string text)
	{
		Text = text;
	}
}
