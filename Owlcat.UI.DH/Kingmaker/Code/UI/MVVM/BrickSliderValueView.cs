using System;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickSliderValueView : ViewBase<BrickSliderValueVM>
{
	[SerializeField]
	private Slider m_Slider;

	[FormerlySerializedAs("Value")]
	[SerializeField]
	public TextMeshProUGUI BottomText;

	[SerializeField]
	public TextMeshProUGUI TopText;

	[SerializeField]
	public Image Image;

	[SerializeField]
	private Image m_FillImage;

	private event Action DoOnDispose;

	protected override void BindViewImplementation()
	{
		BottomText.text = "";
		TopText.text = "";
		m_Slider.minValue = base.ViewModel.MinValue;
		m_Slider.maxValue = base.ViewModel.MaxValue;
		m_Slider.value = base.ViewModel.Value;
		Image.gameObject.SetActive(base.ViewModel.Sprite != null);
		Image.sprite = base.ViewModel.Sprite;
		m_FillImage.enabled = base.ViewModel.NeedColor;
		m_FillImage.color = base.ViewModel.BgrColor;
		if (base.ViewModel.NeedValueText)
		{
			TextMeshProUGUI valueTMP = (base.ViewModel.IsValueOnBottom ? BottomText : TopText);
			valueTMP.text = m_Slider.value.ToString();
			if (base.ViewModel.ValueColor.HasValue)
			{
				Color defaultValueColor = valueTMP.color;
				DoOnDispose += delegate
				{
					valueTMP.color = defaultValueColor;
				};
				valueTMP.color = base.ViewModel.ValueColor.Value;
			}
		}
		TextMeshProUGUI textTMP = (base.ViewModel.IsValueOnBottom ? TopText : BottomText);
		if (base.ViewModel.Text != null)
		{
			textTMP.text = base.ViewModel.Text;
		}
		if (base.ViewModel.TextColor.HasValue)
		{
			Color defaultTextColor = textTMP.color;
			DoOnDispose += delegate
			{
				textTMP.color = defaultTextColor;
			};
			textTMP.color = base.ViewModel.TextColor.Value;
		}
	}

	protected override void DestroyViewImplementation()
	{
		base.gameObject.SetActive(value: false);
		this.DoOnDispose?.Invoke();
		this.DoOnDispose = null;
	}
}
