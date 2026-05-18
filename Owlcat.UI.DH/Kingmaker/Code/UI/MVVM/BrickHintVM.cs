namespace Kingmaker.Code.UI.MVVM;

public class BrickHintVM : TooltipBrickVM
{
	public readonly string Text;

	public BrickHintVM(string text)
	{
		Text = text;
	}
}
