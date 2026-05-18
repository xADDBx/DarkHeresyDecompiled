using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public readonly struct CostStruct
{
	public readonly string LeftText;

	public readonly string RightText;

	public readonly string AdditionalText;

	public readonly TooltipBaseTemplate Tooltip;

	public readonly CostType CostType;

	public CostStruct(string leftText, string rightText, string additionalText, CostType costType, TooltipBaseTemplate tooltip = null)
	{
		LeftText = leftText;
		RightText = rightText;
		AdditionalText = additionalText;
		Tooltip = tooltip;
		CostType = costType;
	}
}
