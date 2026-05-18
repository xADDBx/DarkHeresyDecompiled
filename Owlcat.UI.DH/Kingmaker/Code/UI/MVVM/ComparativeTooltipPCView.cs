using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

public class ComparativeTooltipPCView : ComparativeTooltipView
{
	private const int VerticalLayoutTooltipHeight = 495;

	protected override void OnBind()
	{
		int height = (base.UseVerticalMainLayout ? 495 : base.ViewModel.MainTooltips[0].MaxHeight);
		OverrideTooltipsGroupHeight(base.ViewModel.MainTooltips, height);
		if (base.ViewModel.CompareTooltips.Count > 0)
		{
			int height2 = (base.UseVerticalComparativeLayout ? 495 : base.ViewModel.CompareTooltips[0].MaxHeight);
			OverrideTooltipsGroupHeight(base.ViewModel.CompareTooltips, height2);
		}
		base.OnBind();
	}

	private void OverrideTooltipsGroupHeight(IReadOnlyList<TooltipVM> tooltips, int height)
	{
		if (tooltips == null || tooltips.Count < 1)
		{
			return;
		}
		foreach (TooltipVM tooltip in tooltips)
		{
			tooltip.OverrideMaxHeight(height);
		}
	}
}
