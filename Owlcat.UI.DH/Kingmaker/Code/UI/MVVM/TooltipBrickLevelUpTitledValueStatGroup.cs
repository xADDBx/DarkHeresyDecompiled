using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickLevelUpTitledValueStatGroup : ITooltipBrick
{
	private readonly string m_Title;

	private readonly List<(string Name, string Value)> m_StatGroups;

	public TooltipBrickLevelUpTitledValueStatGroup(string title, List<(string Name, string Value)> statGroups)
	{
		m_Title = title;
		m_StatGroups = statGroups;
	}

	public virtual TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickLevelUpTitledValueStatGroupVM(m_Title, m_StatGroups);
	}
}
