using System.Collections.Generic;
using Assets.Code.View.UI.MVVM;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpSkillcheckBonusVM : TooltipBaseBrickVM
{
	public readonly string BonusValue;

	public readonly List<TooltipElementRelatedSkillVM> RelatedSkills = new List<TooltipElementRelatedSkillVM>();

	public TooltipBrickLevelUpSkillcheckBonusVM(string bonusValue, string attributeAcronym, List<string> relatedSkills)
	{
		TooltipBrickLevelUpSkillcheckBonusVM tooltipBrickLevelUpSkillcheckBonusVM = this;
		BonusValue = bonusValue;
		relatedSkills.ForEach(delegate(string skill)
		{
			tooltipBrickLevelUpSkillcheckBonusVM.RelatedSkills.Add(new TooltipElementRelatedSkillVM(skill, attributeAcronym));
		});
	}
}
