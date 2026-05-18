using System.Collections.Generic;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BrickChargenSelectionRestrictionsVM : TooltipBrickVM
{
	public readonly IReadOnlyList<AbilityRestrictionGroupVM> Groups;

	public readonly bool IsPassed;

	public readonly string HeaderText;

	public BrickChargenSelectionRestrictionsVM(IReadOnlyList<AbilityRestrictionGroupVM> groups, string headerText)
	{
		HeaderText = headerText;
		Groups = groups;
		bool isPassed = true;
		for (int i = 0; i < groups.Count; i++)
		{
			groups[i].AddTo(this);
			if (!groups[i].IsPassed)
			{
				isPassed = false;
			}
			if (i > 0)
			{
				groups[i].SetPreviousGroup(groups[i - 1]);
			}
			if (i < groups.Count - 1)
			{
				groups[i].SetNextGroup(groups[i + 1]);
			}
		}
		IsPassed = isPassed;
	}
}
