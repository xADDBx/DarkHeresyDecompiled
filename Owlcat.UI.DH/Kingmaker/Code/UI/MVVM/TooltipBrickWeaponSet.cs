using Kingmaker.Items.Slots;
using Owlcat.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickWeaponSet : ITooltipBrick
{
	private HandSlot m_HandSlot;

	private bool m_IsPrimary;

	public TooltipBrickWeaponSet(HandSlot handSlot, bool isPrimary)
	{
		m_HandSlot = handSlot;
		m_IsPrimary = isPrimary;
	}

	public TooltipBaseBrickVM GetVM()
	{
		return new TooltipBrickWeaponSetVM(m_HandSlot, m_IsPrimary);
	}
}
