namespace Kingmaker.Code.UI.MVVM;

public class BrickValueStatFormulaVM : TooltipBrickVM
{
	public readonly string Value;

	public readonly string Symbol;

	public readonly string Name;

	public BrickValueStatFormulaVM(string value, string symbol, string name)
	{
		Value = value;
		Symbol = symbol;
		Name = name;
	}
}
