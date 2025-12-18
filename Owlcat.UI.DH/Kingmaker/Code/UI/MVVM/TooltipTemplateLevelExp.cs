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
		yield return new TooltipBrickTitle(m_LevelGlossaryEntry?.Title, TooltipTitleType.H1);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		List<ITooltipBrick> list = new List<ITooltipBrick>();
		UITooltips tooltips = UIStrings.Instance.Tooltips;
		int currentValue = m_ExperienceVM.CurrentExp.CurrentValue;
		int currentValue2 = m_ExperienceVM.NextLevelExp.CurrentValue;
		int num = currentValue2 - currentValue;
		list.Add(new TooltipBricksGroupStart());
		list.Add(new TooltipBrickIconStatValue(tooltips.CurrentLevelExperience, $"{currentValue}"));
		list.Add(new TooltipBrickIconStatValue(tooltips.NextLevelExperience, $"{currentValue2}"));
		if (num > 0)
		{
			list.Add(new TooltipBrickIconStatValue(tooltips.TillNextLevelExperience, $"{num}"));
		}
		list.Add(new TooltipBricksGroupEnd());
		list.Add(new TooltipBrickText(m_LevelGlossaryEntry?.GetDescription()));
		return list;
	}
}
