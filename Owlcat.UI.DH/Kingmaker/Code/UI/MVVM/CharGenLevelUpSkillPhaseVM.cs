using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup.Selections.Stats;

namespace Kingmaker.Code.UI.MVVM;

public class CharGenLevelUpSkillPhaseVM : CharGenLevelUpBaseStatsPhaseVM<CharGenLevelUpCharacteristicsItemVM>
{
	public CharGenLevelUpSkillPhaseVM(CharGenContext charGenContext, SelectionStateStats selectionStats, InfoSectionVM infoSectionVM, int rank = 0)
		: base(charGenContext, selectionStats, CharGenPhaseType.LevelUpSkill, infoSectionVM, rank)
	{
	}
}
