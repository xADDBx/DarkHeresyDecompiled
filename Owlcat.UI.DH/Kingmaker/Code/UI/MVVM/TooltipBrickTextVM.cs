using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTextVM : TooltipBaseBrickVM
{
	public readonly string Text;

	public readonly TooltipTextType Type;

	public readonly bool IsHeader;

	public readonly TooltipTextAlignment Alignment;

	public readonly bool NeedChangeSize;

	public readonly int TextSize;

	public readonly bool IsOverline;

	public TooltipBrickTextVM(string text, TooltipTextType type, TooltipTextAlignment alignment = TooltipTextAlignment.Midl, bool isHeader = false, bool needChangeSize = false, int textSize = 18, bool isOverline = false)
	{
		Text = text;
		Type = type;
		IsHeader = isHeader;
		Alignment = alignment;
		NeedChangeSize = needChangeSize;
		TextSize = textSize;
		IsOverline = isOverline;
	}
}
