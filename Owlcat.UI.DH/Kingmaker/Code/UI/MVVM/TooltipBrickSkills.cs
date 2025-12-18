using Kingmaker.EntitySystem.Stats;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSkills : ITooltipBrick
{
	private readonly CharInfoSkillsBlockVM m_Skills;

	public TooltipBrickSkills(StatsContainer stats)
	{
		m_Skills = new CharInfoSkillsBlockVM(stats);
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickSkillsVM(m_Skills);
	}
}
