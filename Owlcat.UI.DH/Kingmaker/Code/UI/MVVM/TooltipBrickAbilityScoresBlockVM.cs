using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickAbilityScoresBlockVM : TooltipBaseBrickVM
{
	public CharInfoAbilityScoresBlockVM AbilityScoresBlock;

	public TooltipBrickAbilityScoresBlockVM(CharInfoAbilityScoresBlockVM abilityScoresBlock)
	{
		AddDisposable(AbilityScoresBlock = abilityScoresBlock);
	}
}
