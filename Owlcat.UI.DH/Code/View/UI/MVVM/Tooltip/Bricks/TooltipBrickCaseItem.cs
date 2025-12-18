using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks;

public class TooltipBrickCaseItem : ITooltipBrick
{
	private readonly BlueprintCaseItem m_BlueprintCaseItem;

	private readonly bool m_ForceHideContradiction;

	public TooltipBrickCaseItem(BlueprintCaseItem blueprintCaseItem, bool forceHideContradiction = true)
	{
		m_BlueprintCaseItem = blueprintCaseItem;
		m_ForceHideContradiction = forceHideContradiction;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickCaseItemVM(m_BlueprintCaseItem, m_ForceHideContradiction);
	}
}
