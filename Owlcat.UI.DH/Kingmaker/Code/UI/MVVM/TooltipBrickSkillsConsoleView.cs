using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSkillsConsoleView : TooltipBrickSkillsView, IConsoleTooltipBrick
{
	public bool IsBinded => base.ViewModel != null;

	public IConsoleEntity GetConsoleEntity()
	{
		if (m_AbilityScoresBlockView is CharInfoSkillsBlockConsoleView charInfoSkillsBlockConsoleView)
		{
			return charInfoSkillsBlockConsoleView.GetConsoleEntity();
		}
		return new GridConsoleNavigationBehaviour();
	}
}
