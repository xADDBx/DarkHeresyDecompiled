using System.Collections.Generic;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateRace : TooltipBaseTemplate
{
	private readonly string m_Name;

	private readonly string m_Desc;

	public TooltipTemplateRace(BlueprintRace race)
	{
		if (race != null)
		{
			m_Name = race.Name;
			m_Desc = race.Description;
		}
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new BrickTitleVM(m_Name);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		yield return new BrickTextVM(m_Desc, TooltipTextType.Paragraph);
	}
}
