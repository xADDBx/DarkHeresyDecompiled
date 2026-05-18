using Kingmaker.Utility.UnitDescription;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class BrickAbilityScoresVM : TooltipBrickVM
{
	public readonly CharInfoAbilityScoresBlockVM AbilityScoresBlock;

	public BrickAbilityScoresVM(UnitDescription.StatsData statsData)
	{
		AbilityScoresBlock = new CharInfoAbilityScoresBlockVM(statsData).AddTo(this);
	}
}
