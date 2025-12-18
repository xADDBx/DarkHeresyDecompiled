using System;

namespace Owlcat.UI;

public interface ITooltipBrick
{
	[Obsolete("Use ITooltipBrick as data source instead")]
	TooltipBaseBrickVM GetVM();
}
