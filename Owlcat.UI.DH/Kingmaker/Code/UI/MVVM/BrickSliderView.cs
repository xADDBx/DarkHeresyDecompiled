using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSliderView : BrickBaseView<BrickSliderVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_MinValue;

	[SerializeField]
	private TMP_Text m_MaxValue;

	[SerializeField]
	private TMP_Text m_MaxValueText;

	[SerializeField]
	private TMP_Text m_CurrentValue;

	[SerializeField]
	private Slider m_Slider;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private WidgetList m_WidgetList;

	[Header("Views")]
	[SerializeField]
	private SliderValuesView m_SliderValuesPrefab;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_MinValue, m_MaxValue, m_MaxValueText, m_CurrentValue).AddTo(this);
		}
		base.OnBind();
		m_Slider.minValue = base.ViewModel.MinValue;
		m_Slider.maxValue = base.ViewModel.MaxValue;
		m_Slider.value = base.ViewModel.Value;
		TMP_Text minValue = m_MinValue;
		int minValue2 = base.ViewModel.MinValue;
		minValue.text = minValue2.ToString();
		TMP_Text maxValue = m_MaxValue;
		minValue2 = base.ViewModel.MaxValue;
		maxValue.text = minValue2.ToString();
		if ((bool)m_MaxValueText && base.ViewModel.MaxValueText != null)
		{
			m_MaxValueText.text = base.ViewModel.MaxValueText;
			m_MaxValueText.gameObject.SetActive(value: true);
		}
		else
		{
			m_MaxValueText.gameObject.SetActive(value: false);
		}
		m_CurrentValue.gameObject.SetActive(base.ViewModel.ShowValue);
		TMP_Text currentValue = m_CurrentValue;
		minValue2 = base.ViewModel.Value;
		currentValue.text = minValue2.ToString();
		m_Image.color = base.ViewModel.FillColor;
		m_WidgetList.DrawEntries(base.ViewModel.SliderValueVMs, m_SliderValuesPrefab).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
