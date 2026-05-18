using Kingmaker.Framework.DetectiveSystem;

namespace Kingmaker.Code.UI.MVVM;

public class BrickCaseItemVM : TooltipBrickVM
{
	public readonly BlueprintCaseItem BlueprintCaseItem;

	public readonly bool HasContradiction;

	public BrickCaseItemVM(BlueprintCaseItem blueprintCaseItem, bool forceHideContradiction = true)
	{
		BlueprintCaseItem = blueprintCaseItem;
		HasContradiction = !forceHideContradiction && ((blueprintCaseItem as BlueprintConclusion)?.IsRefuted() ?? false);
	}
}
