using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSpaceView : TooltipBaseBrickView<TooltipBrickSpaceVM>
{
	[SerializeField]
	private LayoutElement m_LayoutElement;

	protected override void OnBind()
	{
		if ((bool)m_LayoutElement && base.ViewModel.Height.HasValue)
		{
			m_LayoutElement.minHeight = base.ViewModel.Height.Value;
		}
	}
}
