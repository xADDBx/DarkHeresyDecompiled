using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpAbilityUpgradeDescriptionVM : TooltipBaseBrickVM
{
	public readonly string BaseAbilityDescription;

	public readonly string UpgradeAbilityDescription;

	public TooltipBrickLevelUpAbilityUpgradeDescriptionVM(string upgradeAbilityDescription, string baseAbilityDescription)
	{
		BaseAbilityDescription = baseAbilityDescription;
		UpgradeAbilityDescription = upgradeAbilityDescription;
	}
}
