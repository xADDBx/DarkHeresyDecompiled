namespace Kingmaker.Code.UI.MVVM;

public class BrickTextValueVM : TooltipBrickVM
{
	public readonly string Text;

	public readonly string Value;

	public readonly int NestedLevel;

	public readonly bool IsResultValue;

	public readonly bool NeedShowLine;

	public BrickTextValueVM(string text, string value, int nestedLevel = 0, bool isResultValue = false, bool needShowLine = true)
	{
		Text = text;
		Value = value;
		NestedLevel = nestedLevel;
		IsResultValue = isResultValue;
		NeedShowLine = needShowLine;
	}
}
