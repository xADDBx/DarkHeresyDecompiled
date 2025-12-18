using System.Collections.Generic;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickPrerequisite : ITooltipBrick
{
	private readonly List<PrerequisiteEntryVM> m_PrerequisiteEntries;

	public TooltipBrickPrerequisite(List<PrerequisiteEntryVM> prerequisiteEntries)
	{
		m_PrerequisiteEntries = prerequisiteEntries;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickPrerequisiteVM(m_PrerequisiteEntries);
	}
}
