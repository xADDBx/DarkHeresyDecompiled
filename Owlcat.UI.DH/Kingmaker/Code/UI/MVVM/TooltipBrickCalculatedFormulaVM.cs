using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickCalculatedFormulaVM : TooltipBaseBrickVM
{
	public readonly string Title;

	public readonly string Description;

	public readonly string Value;

	public readonly bool UseFlexibleValueWidth;

	public TooltipBrickCalculatedFormulaVM(string title, string description, string value, bool useFlexibleValueWidth)
	{
		Title = title;
		Description = description;
		Value = value;
		UseFlexibleValueWidth = useFlexibleValueWidth;
	}
}
