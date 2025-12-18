using Code.View.UI.Helpers;
using Code.View.UI.MVVM.Tooltip.Bricks;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickTwoColumnsStatView : TooltipBaseBrickView<TooltipBrickTwoColumnsStatVM>
{
	[SerializeField]
	private StatDataWidget m_LeftStatWidget;

	[SerializeField]
	private StatDataWidget m_RightStatWidget;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper();
		m_TextHelper.AppendTexts(m_LeftStatWidget.GetTexts());
		m_TextHelper.AppendTexts(m_RightStatWidget.GetTexts());
		m_LeftStatWidget.Bind(base.ViewModel.LeftStat);
		m_RightStatWidget.Bind(base.ViewModel.RightStat);
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_LeftStatWidget.Unbind();
		m_RightStatWidget.Unbind();
		m_TextHelper.Dispose();
		m_TextHelper = null;
	}
}
