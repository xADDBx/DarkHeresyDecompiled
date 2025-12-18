using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickUnifiedStatusVM : TooltipBaseBrickVM
{
	public readonly UnifiedStatus Status;

	public readonly string Text;

	public TooltipBrickUnifiedStatusVM(UnifiedStatus status, string text)
	{
		Status = status;
		Text = text;
	}
}
