using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpHeader : ITooltipBrick
{
	private readonly TooltipBrickLevelUpFeatureData m_Data;

	private readonly TooltipBrickLevelUpFeatureData m_Subheader;

	private readonly TooltipLevelUpAbilityData m_AbilityData;

	public TooltipBrickLevelUpHeader(TooltipBrickLevelUpFeatureData data, TooltipBrickLevelUpFeatureData subheader = null, TooltipLevelUpAbilityData abilityData = null)
	{
		m_Data = data;
		m_Subheader = subheader;
		m_AbilityData = abilityData;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickLevelUpHeaderVM(m_Data, m_Subheader, m_AbilityData);
	}
}
