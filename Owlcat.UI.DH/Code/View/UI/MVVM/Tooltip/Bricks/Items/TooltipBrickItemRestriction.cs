using System.Collections.Generic;
using Owlcat.UI;

namespace Code.View.UI.MVVM.Tooltip.Bricks.Items;

public class TooltipBrickItemRestriction : ITooltipBrick
{
	private readonly List<RestrictionData> m_RestrictionData;

	private readonly bool m_CanEquip;

	public TooltipBrickItemRestriction(List<RestrictionData> restrictionData, bool canEquip)
	{
		m_RestrictionData = restrictionData;
		m_CanEquip = canEquip;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickItemRestrictionVM(m_RestrictionData, m_CanEquip);
	}
}
