using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipTemplateGlobalMapPosition : TooltipBaseTemplate
{
	public override IEnumerable<ITooltipBrick> GetBody(TooltipTemplateType type)
	{
		return new List<ITooltipBrick>
		{
			new TooltipBrickGlobalMapPosition()
		};
	}
}
