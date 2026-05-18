using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipElementStatValueVM : ViewModel
{
	public readonly string StatName;

	public readonly string Value;

	public TooltipElementStatValueVM(string statName, string value)
	{
		StatName = statName;
		Value = value;
	}
}
