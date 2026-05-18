using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTwoColumnsStatView : BrickBaseView<BrickTwoColumnsStatVM>
{
	[SerializeField]
	private StatDataWidget m_LeftStatWidget;

	[SerializeField]
	private StatDataWidget m_RightStatWidget;

	protected override void OnBind()
	{
		m_LeftStatWidget.Bind(base.ViewModel.LeftStat);
		m_RightStatWidget.Bind(base.ViewModel.RightStat);
	}
}
