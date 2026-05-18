using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickItemCostVM : TooltipBrickVM
{
	public readonly string LeftText;

	public readonly string RightText;

	public readonly string AdditionalLine;

	public readonly CostType CostType;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickItemCostVM(CostStruct cost)
	{
		LeftText = cost.LeftText;
		RightText = cost.RightText;
		AdditionalLine = cost.AdditionalText;
		CostType = cost.CostType;
		Tooltip = cost.Tooltip;
	}
}
