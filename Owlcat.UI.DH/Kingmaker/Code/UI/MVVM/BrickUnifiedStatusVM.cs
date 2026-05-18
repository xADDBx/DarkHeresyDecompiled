namespace Kingmaker.Code.UI.MVVM;

public class BrickUnifiedStatusVM : TooltipBrickVM
{
	public readonly UnifiedStatus Status;

	public readonly string Text;

	public BrickUnifiedStatusVM(UnifiedStatus status, string text)
	{
		Status = status;
		Text = text;
	}
}
