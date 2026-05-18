using System.Collections.Generic;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Encyclopedia;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateLevelExp : TooltipBaseTemplate
{
	private readonly CharInfoExperienceVM m_ExperienceVM;

	private readonly BlueprintEncyclopediaGlossaryEntry m_LevelGlossaryEntry;

	public TooltipTemplateLevelExp(CharInfoExperienceVM experienceVM)
	{
		m_ExperienceVM = experienceVM;
		m_LevelGlossaryEntry = UIUtilityEncyclopedy.GetGlossaryEntry("CharacterLevel");
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(m_LevelGlossaryEntry?.Title, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		int currentValue = m_ExperienceVM.CurrentExp.CurrentValue;
		int currentValue2 = m_ExperienceVM.NextLevelExp.CurrentValue;
		int num = currentValue2 - currentValue;
		List<TooltipBrickVM> list2 = new List<TooltipBrickVM>
		{
			new BrickIconStatValueVM(new TextValueAddElement(tooltips.CurrentLevelExperience, currentValue.ToString())),
			new BrickIconStatValueVM(new TextValueAddElement(tooltips.NextLevelExperience, currentValue2.ToString()))
		};
		if (num > 0)
		{
			list2.Add(new BrickIconStatValueVM(new TextValueAddElement(tooltips.TillNextLevelExperience, num.ToString())));
		}
		list.Add(new BricksGroupOneColumnVM(list2));
		list.Add(new BrickTextVM(m_LevelGlossaryEntry?.GetDescription()));
		return list;
	}
}
