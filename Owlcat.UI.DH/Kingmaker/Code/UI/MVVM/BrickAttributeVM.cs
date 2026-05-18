using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAttributeVM : TooltipBrickVM
{
	public readonly string Name;

	public readonly string Acronym;

	public readonly bool IsRecommended;

	public readonly StripeType StripeType;

	public readonly TooltipBaseTemplate Tooltip;

	public BrickAttributeVM(string name, string acronym, TooltipBaseTemplate tooltip, StripeType type, bool isRecommended)
	{
		Name = name;
		Acronym = acronym;
		StripeType = type;
		IsRecommended = isRecommended;
		Tooltip = tooltip;
	}
}
