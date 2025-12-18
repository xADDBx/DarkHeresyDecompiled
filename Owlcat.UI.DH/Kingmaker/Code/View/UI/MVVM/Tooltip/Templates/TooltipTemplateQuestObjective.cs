using System.Collections.Generic;
using Kingmaker.Blueprints.Quests;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.UI;

namespace Kingmaker.Code.View.UI.MVVM.Tooltip.Templates;

public class TooltipTemplateQuestObjective : TooltipBaseTemplate
{
	private readonly BlueprintQuestObjective m_Objective;

	private readonly IEnumerable<BlueprintQuestObjective> m_Addendums;

	public TooltipTemplateQuestObjective(BlueprintQuestObjective objective, IEnumerable<BlueprintQuestObjective> addendums = null)
	{
		m_Objective = objective;
		m_Addendums = addendums;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(m_Objective.Title.Text);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new TooltipBrickText(m_Objective.Description.Text, TooltipTextType.Paragraph);
		if (m_Addendums == null)
		{
			yield break;
		}
		yield return new TooltipBrickSeparator(TooltipBrickElementType.Small);
		foreach (BlueprintQuestObjective addendum in m_Addendums)
		{
			if (!string.IsNullOrEmpty(m_Objective.Description.Text))
			{
				string text = string.Format(UIConfig.Instance.TextFormats.QuestionFormat, addendum.Description.Text);
				yield return new TooltipBrickText(text, TooltipTextType.Paragraph);
			}
		}
	}
}
