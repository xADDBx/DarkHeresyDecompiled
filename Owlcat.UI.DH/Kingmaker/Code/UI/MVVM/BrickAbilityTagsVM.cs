using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityTagsVM : TooltipBrickVM
{
	public readonly IReadOnlyList<string> Tags;

	public BrickAbilityTagsVM(IReadOnlyList<string> tags)
	{
		Tags = tags;
	}
}
