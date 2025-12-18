using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSkillsVM : TooltipBaseBrickVM
{
	public CharInfoSkillsBlockVM AbilityScoresBlock;

	public TooltipBrickSkillsVM(CharInfoSkillsBlockVM abilityScoresBlock)
	{
		AddDisposable(AbilityScoresBlock = abilityScoresBlock);
	}
}
