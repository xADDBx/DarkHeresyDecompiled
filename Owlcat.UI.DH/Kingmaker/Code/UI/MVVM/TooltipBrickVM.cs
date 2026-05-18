using System;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickVM : ViewModel, ITooltipBrick
{
	public virtual TooltipBrickVM Clone()
	{
		return (TooltipBrickVM)MemberwiseClone();
	}

	[Obsolete]
	public TooltipBaseBrickVM GetVM()
	{
		return null;
	}
}
