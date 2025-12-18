using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickValueStatFormulaVM : TooltipBaseBrickVM
{
	public readonly string Value;

	public readonly string Symbol;

	public readonly string Name;

	public TooltipBrickValueStatFormulaVM(string value, string symbol, string name)
	{
		Value = value;
		Symbol = symbol;
		Name = name;
	}
}
