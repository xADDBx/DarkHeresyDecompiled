using Owlcat.UI;

namespace Assets.Code.View.UI.MVVM;

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
