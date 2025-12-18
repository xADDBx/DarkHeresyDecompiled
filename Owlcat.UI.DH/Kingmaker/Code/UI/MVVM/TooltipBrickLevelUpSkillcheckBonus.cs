using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpSkillcheckBonus : ITooltipBrick
{
	private readonly string m_BonusValue;

	private readonly string m_AttributeAcronym;

	private readonly List<string> m_relatedSkills;

	public TooltipBrickLevelUpSkillcheckBonus(string bonusValue, string attributeAcronym, List<string> relatedSkills)
	{
		m_BonusValue = bonusValue;
		m_AttributeAcronym = attributeAcronym;
		m_relatedSkills = relatedSkills;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickLevelUpSkillcheckBonusVM(m_BonusValue, m_AttributeAcronym, m_relatedSkills);
	}
}
