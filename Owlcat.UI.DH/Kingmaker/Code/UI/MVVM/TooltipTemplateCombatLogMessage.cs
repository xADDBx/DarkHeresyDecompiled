using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateCombatLogMessage : TooltipTemplateSimple
{
	public IEnumerable<ITooltipBrick> ExtraTooltipBricks = Enumerable.Empty<ITooltipBrick>();

	public IEnumerable<ITooltipBrick> ExtraInfoBricks = Enumerable.Empty<ITooltipBrick>();

	public TooltipTemplateCombatLogMessage(string header, string description, float contentSpacing = 0f)
		: base(header, description)
	{
		ContentSpacing = contentSpacing;
	}

	public override IEnumerable<ITooltipBrick> GetHeader(TooltipTemplateType type)
	{
		yield return new TooltipBrickTitle(base.Header, TooltipTitleType.H4, TextAlignmentOptions.Center, TextAnchor.MiddleCenter, 4);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		if (type == TooltipTemplateType.Tooltip)
		{
			return ExtraTooltipBricks.Concat(base.GetBody(type));
		}
		return ExtraInfoBricks.Concat(base.GetBody(type));
	}
}
