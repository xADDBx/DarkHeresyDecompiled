using System.Collections.Generic;

namespace Kingmaker.Code.UI.MVVM;

public class BrickPrerequisiteVM : TooltipBrickVM
{
	public readonly List<PrerequisiteEntryVM> PrerequisiteEntries;

	public BrickPrerequisiteVM(List<PrerequisiteEntryVM> prerequisiteEntries)
	{
		PrerequisiteEntries = prerequisiteEntries;
	}

	protected override void OnDispose()
	{
		base.OnDispose();
		foreach (PrerequisiteEntryVM prerequisiteEntry in PrerequisiteEntries)
		{
			prerequisiteEntry.Dispose();
		}
		PrerequisiteEntries.Clear();
	}
}
