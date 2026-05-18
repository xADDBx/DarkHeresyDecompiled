using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

public class BrickLevelUpRelatedSkillsVM : TooltipBrickVM
{
	public readonly List<TooltipElementRelatedSkillVM> RelatedSkills = new List<TooltipElementRelatedSkillVM>();

	public BrickLevelUpRelatedSkillsVM(string attributeAcronym, List<string> relatedSkills)
	{
		BrickLevelUpRelatedSkillsVM brickLevelUpRelatedSkillsVM = this;
		relatedSkills.ForEach(delegate(string skill)
		{
			brickLevelUpRelatedSkillsVM.RelatedSkills.Add(new TooltipElementRelatedSkillVM(skill, attributeAcronym));
		});
	}
}
