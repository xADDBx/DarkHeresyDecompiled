using System.Collections.Generic;
using Kingmaker.UnitLogic.Levelup.Selections;

namespace Kingmaker.Code.UI.MVVM;

public static class RankEntrySelectionUtils
{
	private static readonly List<FeatureGroup> GroupsWithFilter = new List<FeatureGroup>
	{
		FeatureGroup.ActiveAbility,
		FeatureGroup.FirstCareerAbility,
		FeatureGroup.SecondCareerAbility,
		FeatureGroup.FirstOrSecondCareerAbility,
		FeatureGroup.Talent,
		FeatureGroup.CommonTalent,
		FeatureGroup.AscensionTalent,
		FeatureGroup.FirstCareerTalent,
		FeatureGroup.SecondCareerTalent,
		FeatureGroup.FirstOrSecondCareerTalent
	};

	public static bool HasFilter(FeatureGroup? group)
	{
		if (group.HasValue)
		{
			return GroupsWithFilter.Contains(group.Value);
		}
		return false;
	}
}
