namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpAbilityUpgradeDescriptionVM : TooltipBrickVM
{
	public readonly string BaseAbilityDescription;

	public readonly string UpgradeAbilityDescription;

	public BrickLevelUpAbilityUpgradeDescriptionVM(string upgradeAbilityDescription, string baseAbilityDescription)
	{
		BaseAbilityDescription = baseAbilityDescription;
		UpgradeAbilityDescription = upgradeAbilityDescription;
	}
}
