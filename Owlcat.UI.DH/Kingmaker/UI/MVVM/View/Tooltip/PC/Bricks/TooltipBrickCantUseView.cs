using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.MVVM.Tooltip.Bricks;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.UI.MVVM.View.Tooltip.PC.Bricks;

public class TooltipBrickCantUseView : TooltipBaseBrickView<TooltipBrickCantUseVM>
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	protected override void OnBind()
	{
		base.OnBind();
		m_Text.text = base.ViewModel.CantUseLabel;
		m_Text.SetLinkTooltip(null, null, new TooltipConfig(InfoCallPCMethod.LeftMouseButton));
	}
}
