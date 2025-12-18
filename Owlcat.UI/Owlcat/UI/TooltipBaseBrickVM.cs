using System;

namespace Owlcat.UI;

[Obsolete("Use source ITooltipBrick instead")]
public abstract class TooltipBaseBrickVM : BaseDisposable, IViewModel, IBaseDisposable, IDisposable
{
	protected override void DisposeImplementation()
	{
	}
}
