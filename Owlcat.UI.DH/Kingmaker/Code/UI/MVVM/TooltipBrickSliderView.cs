using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickSliderView : TooltipBaseBrickView<TooltipBrickSliderVM>
{
	[SerializeField]
	private BrickSliderValueView m_ValueViewPrefab;

	[SerializeField]
	private Transform m_ValueViewsContainer;

	[SerializeField]
	protected List<BrickSliderValueView> m_SliderValueViews = new List<BrickSliderValueView>();

	[SerializeField]
	private Slider m_Slider;

	[SerializeField]
	private Image m_Image;

	[SerializeField]
	private TextMeshProUGUI m_MinValue;

	[SerializeField]
	private TextMeshProUGUI m_MaxValue;

	[SerializeField]
	private TextMeshProUGUI m_MaxValueText;

	[SerializeField]
	private TextMeshProUGUI m_CurrentValue;

	[SerializeField]
	private LayoutElement m_LayoutElement;

	[SerializeField]
	private float m_DefaultFontSize = 16f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 16f;

	protected override void OnBind()
	{
		base.OnBind();
		m_Slider.minValue = base.ViewModel.MinValue;
		m_Slider.maxValue = base.ViewModel.MaxValue;
		m_Slider.value = base.ViewModel.Value;
		m_MinValue.text = base.ViewModel.MinValue.ToString();
		m_MaxValue.text = base.ViewModel.MaxValue.ToString();
		if ((bool)m_MaxValueText && base.ViewModel.MaxValueText != null)
		{
			m_MaxValueText.text = base.ViewModel.MaxValueText.Text;
			m_MaxValueText.gameObject.SetActive(value: true);
		}
		else
		{
			m_MaxValueText.gameObject.SetActive(value: false);
		}
		m_CurrentValue.gameObject.SetActive(base.ViewModel.ShowValue);
		m_CurrentValue.text = base.ViewModel.Value.ToString();
		m_Image.color = base.ViewModel.FillColor;
		m_LayoutElement.preferredHeight = base.ViewModel.PreferredHeight;
		for (int i = m_SliderValueViews.Count; i < base.ViewModel.SliderValueVMs.Count; i++)
		{
			BrickSliderValueView brickSliderValueView = Object.Instantiate(m_ValueViewPrefab, m_ValueViewsContainer, worldPositionStays: false);
			brickSliderValueView.gameObject.SetActive(value: false);
			brickSliderValueView.transform.SetAsFirstSibling();
			m_SliderValueViews.Add(brickSliderValueView);
		}
		for (int j = 0; j < base.ViewModel.SliderValueVMs.Count; j++)
		{
			if (base.ViewModel.SliderValueVMs[j] != null)
			{
				m_SliderValueViews[j].Bind(base.ViewModel.SliderValueVMs[j]);
				m_SliderValueViews[j].gameObject.SetActive(value: true);
			}
		}
		bool isControllerMouse = Game.Instance.IsControllerMouse;
		m_CurrentValue.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
		m_MaxValue.fontSize = (isControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
	}

	protected override void OnUnbind()
	{
		foreach (BrickSliderValueView sliderValueView in m_SliderValueViews)
		{
			base.OnUnbind();
			sliderValueView.gameObject.SetActive(value: false);
		}
	}
}
