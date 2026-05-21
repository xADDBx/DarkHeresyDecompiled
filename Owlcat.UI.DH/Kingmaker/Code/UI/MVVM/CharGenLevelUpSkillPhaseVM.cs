using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSkillPhaseVM : CharGenLevelUpBaseStatsPhaseVM<CharGenLevelUpCharacteristicsItemVM>
{
	public CharGenLevelUpSkillPhaseVM(CharGenContext charGenContext, SelectionStateStats selectionStats, InfoSectionVM infoSectionVM, PartyStatsOverviewVM partyStatsOverviewVM, int rank = 0)
		: base(charGenContext, selectionStats, CharGenPhaseType.LevelUpSkill, infoSectionVM, partyStatsOverviewVM, rank)
	{
		CreateBaseAttributeList();
	}

	private void CreateBaseAttributeList()
	{
		IEnumerable<StatType> sortedStats = from a in base.SelectionStats.Blueprint.Advancements
			orderby StatTypeHelper.DisplayOrder.IndexOf(a.Stat)
			select MechanicActor.GetStatBaseStat(a.Stat).GetValueOrDefault();
		StatType[] distinctList = sortedStats.Distinct().ToArray();
		distinctList.ForEach(delegate(StatType s)
		{
			BaseAttributeList.Add(new LevelUpSkillLinkedAttributeVM(s, sortedStats.Count((StatType stat) => s == stat), distinctList.IndexOf(s) % 2 == 0));
		});
	}
}
