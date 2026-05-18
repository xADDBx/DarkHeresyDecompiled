namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenDividerTextLineVM : TooltipBrickVM
{
	public readonly DividerType DividerType;

	public readonly string Text;

	public BrickChargenDividerTextLineVM(DividerType dividerType, string text = null)
	{
		DividerType = dividerType;
		Text = text;
	}
}
