using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarSlotWeaponPCView : ActionBarSlotWeaponView
{
	protected override void OnBind()
	{
		base.OnBind();
		this.SetTooltip(base.ViewModel.Tooltip, new TooltipConfig(InfoCallPCMethod.RightMouseButton, InfoCallConsoleMethod.LongRightStickButton, isGlossary: false, isEncyclopedia: false, base.TooltipPlace, 0, 0, 0, m_TooltipPriorityPivots)).AddTo(this);
	}
}
