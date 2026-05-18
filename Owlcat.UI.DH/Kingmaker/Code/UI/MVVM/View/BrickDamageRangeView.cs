using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class BrickDamageRangeView : BrickCombatLogBaseView<BrickDamageRangeVM>
{
	[Header("Slider")]
	[SerializeField]
	private Slider m_CurrentValueSlider;

	[SerializeField]
	private TMP_Text m_CurrentValueText;

	[SerializeField]
	private TMP_Text m_MinValueText;

	[SerializeField]
	private TMP_Text m_MaxValueText;

	protected override void OnBind()
	{
		base.OnBind();
		m_CurrentValueSlider.minValue = base.ViewModel.MinValue;
		m_CurrentValueSlider.maxValue = base.ViewModel.MaxValue;
		m_CurrentValueSlider.value = base.ViewModel.CurrentValue;
		TMP_Text currentValueText = m_CurrentValueText;
		int currentValue = base.ViewModel.CurrentValue;
		currentValueText.text = currentValue.ToString();
		TMP_Text minValueText = m_MinValueText;
		currentValue = base.ViewModel.MinValue;
		minValueText.text = currentValue.ToString();
		TMP_Text maxValueText = m_MaxValueText;
		currentValue = base.ViewModel.MaxValue;
		maxValueText.text = currentValue.ToString();
	}
}
