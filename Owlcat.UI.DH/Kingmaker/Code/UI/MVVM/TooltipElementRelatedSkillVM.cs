using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipElementRelatedSkillVM : ViewModel
{
	public readonly string SkillName;

	public readonly string AttributeAcronym;

	public TooltipElementRelatedSkillVM(string skillName, string attributeAcronym)
	{
		SkillName = skillName;
		AttributeAcronym = attributeAcronym;
	}
}
