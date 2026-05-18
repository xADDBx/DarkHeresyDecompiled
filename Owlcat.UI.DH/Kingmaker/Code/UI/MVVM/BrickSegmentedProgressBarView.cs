using System;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSegmentedProgressBarView : BrickBaseView<BrickSegmentedProgressBarVM>
{
	[SerializeField]
	private TMP_Text m_TitleText;

	[SerializeField]
	private TMP_Text m_CurrentValueText;

	[SerializeField]
	private DoubleSidedSegmentedProgressBar m_ProgressBar;

	[SerializeField]
	private MonoBehaviour m_TooltipSource;

	private IDisposable m_Tooltip;

	protected override void OnBind()
	{
		base.OnBind();
		m_TitleText.SetText(base.ViewModel.Title);
		TMP_Text currentValueText = m_CurrentValueText;
		int currentValue = base.ViewModel.CurrentValue;
		currentValueText.SetText(currentValue.ToString());
		m_ProgressBar.SetValue(base.ViewModel.MinValue, base.ViewModel.MaxValue, base.ViewModel.CurrentValue);
		m_TooltipSource.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
	}
}
