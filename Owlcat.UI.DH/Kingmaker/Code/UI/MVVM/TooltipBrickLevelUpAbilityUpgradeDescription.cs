using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpAbilityUpgradeDescription : ITooltipBrick
{
	private string m_UpgradeAbilityDescription;

	private string m_BaseAbilityDescription;

	public TooltipBrickLevelUpAbilityUpgradeDescription(string upgradeAbilityDescription, string baseAbilityDescription)
	{
		m_UpgradeAbilityDescription = upgradeAbilityDescription;
		m_BaseAbilityDescription = baseAbilityDescription;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickLevelUpAbilityUpgradeDescriptionVM(m_UpgradeAbilityDescription, m_BaseAbilityDescription);
	}
}
