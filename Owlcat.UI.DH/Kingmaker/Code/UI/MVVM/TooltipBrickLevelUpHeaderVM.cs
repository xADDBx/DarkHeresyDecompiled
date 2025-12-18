using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpHeaderVM : TooltipBaseBrickVM
{
	public readonly TooltipBrickLevelUpFeatureData Data;

	public readonly TooltipBrickLevelUpHeaderVM SubheaderVM;

	public readonly TooltipLevelUpAbilityData AbilityData;

	public TooltipBrickLevelUpHeaderVM(TooltipBrickLevelUpFeatureData data, TooltipBrickLevelUpFeatureData subheader = null, TooltipLevelUpAbilityData abilityData = null)
	{
		Data = data;
		if (subheader != null)
		{
			SubheaderVM = new TooltipBrickLevelUpHeaderVM(subheader);
		}
		AbilityData = abilityData;
	}
}
