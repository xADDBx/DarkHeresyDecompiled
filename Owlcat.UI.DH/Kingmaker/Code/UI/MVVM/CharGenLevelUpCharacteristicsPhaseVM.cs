using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpCharacteristicsPhaseVM : CharGenLevelUpBaseStatsPhaseVM<CharGenLevelUpCharacteristicsItemVM>
{
	public CharGenLevelUpCharacteristicsPhaseVM(CharGenContext charGenContext, SelectionStateStats selectionStats, InfoSectionVM infoSectionVM, PartyStatsOverviewVM partyStatsOverviewVM, int rank = 0)
		: base(charGenContext, selectionStats, CharGenPhaseType.Characteristics, infoSectionVM, partyStatsOverviewVM, rank)
	{
	}
}
