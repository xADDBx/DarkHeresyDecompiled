using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSegmentedProgressBarVM : TooltipBrickVM
{
	public readonly string Title;

	public readonly int MinValue;

	public readonly int MaxValue;

	public readonly int CurrentValue;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickSegmentedProgressBarVM(string title, int min, int max, int current, TooltipBaseTemplate tooltip)
	{
		Title = title;
		MinValue = min;
		MaxValue = max;
		CurrentValue = current;
		Tooltip = tooltip;
	}
}
