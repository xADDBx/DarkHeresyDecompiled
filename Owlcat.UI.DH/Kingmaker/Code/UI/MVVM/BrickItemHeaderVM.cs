namespace Kingmaker.Code.UI.MVVM;

public class BrickItemHeaderVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly ItemHeaderType Type;

	public BrickItemHeaderVM(string text, ItemHeaderType type = ItemHeaderType.Default)
	{
		Text = text;
		Type = type;
	}
}
