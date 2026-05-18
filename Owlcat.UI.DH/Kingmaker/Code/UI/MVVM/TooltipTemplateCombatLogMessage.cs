using System.Collections.Generic;
using System.Linq;
using Owlcat.UI;

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
		yield return new BrickTitleVM(base.Header, TooltipTitleType.H4);
	}

	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return ((type == TooltipTemplateType.Tooltip) ? ExtraTooltipBricks : ExtraInfoBricks).Select((ITooltipBrick brick) => (brick as TooltipBrickVM)?.Clone()).Concat(base.GetBody(type));
	}
}
