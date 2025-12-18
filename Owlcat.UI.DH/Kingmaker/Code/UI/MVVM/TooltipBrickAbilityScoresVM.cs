using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickAbilityScoresVM : TooltipBaseBrickVM
{
	public CharInfoAbilityScoresBlockVM AbilityScoresBlock;

	public TooltipBrickAbilityScoresVM(CharInfoAbilityScoresBlockVM abilityScoresBlock)
	{
		AddDisposable(AbilityScoresBlock = abilityScoresBlock);
	}
}
