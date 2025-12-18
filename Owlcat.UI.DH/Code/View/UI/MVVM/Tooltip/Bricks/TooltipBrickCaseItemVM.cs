using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks;

public class TooltipBrickCaseItemVM : TooltipBaseBrickVM
{
	public readonly BlueprintCaseItem BlueprintCaseItem;

	public readonly bool HasContradiction;

	public TooltipBrickCaseItemVM(BlueprintCaseItem blueprintCaseItem, bool forceHideContradiction)
	{
		BlueprintCaseItem = blueprintCaseItem;
		HasContradiction = !forceHideContradiction && ((blueprintCaseItem as BlueprintConclusion)?.IsRefuted() ?? false);
	}
}
