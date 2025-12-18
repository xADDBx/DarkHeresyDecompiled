using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickItemHeaderVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly ItemHeaderType Type;

	public TooltipBrickItemHeaderVM(string text, ItemHeaderType type)
	{
		Text = text;
		Type = type;
	}
}
