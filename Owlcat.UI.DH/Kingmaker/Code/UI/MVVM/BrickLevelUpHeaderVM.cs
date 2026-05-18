namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpHeaderVM : TooltipBrickVM
{
	public readonly LevelUpFeatureUIData UIData;

	public readonly BrickLevelUpHeaderVM SubheaderVM;

	public readonly TooltipLevelUpAbilityData AbilityData;

	public BrickLevelUpHeaderVM(LevelUpFeatureUIData uiData, LevelUpFeatureUIData subheader = null, TooltipLevelUpAbilityData abilityData = null)
	{
		UIData = uiData;
		if (subheader != null)
		{
			SubheaderVM = new BrickLevelUpHeaderVM(subheader);
		}
		AbilityData = abilityData;
	}
}
