using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class SliderValuesView : View<SliderValuesVM>
{
	[SerializeField]
	public TMP_Text BottomText;

	[SerializeField]
	public TMP_Text TopText;

	[SerializeField]
	private Slider m_Slider;

	[SerializeField]
	public Image Image;

	[SerializeField]
	private Image m_FillImage;

	protected override void OnBind()
	{
		BottomText.text = "";
		TopText.text = "";
		m_Slider.minValue = base.ViewModel.MinValue;
		m_Slider.maxValue = base.ViewModel.MaxValue;
		m_Slider.value = base.ViewModel.Value;
		Image.sprite = base.ViewModel.Sprite;
		m_FillImage.enabled = base.ViewModel.NeedColor;
		m_FillImage.color = base.ViewModel.BgrColor;
		if (base.ViewModel.NeedValueText)
		{
			TMP_Text valueTMP = (base.ViewModel.IsValueOnBottom ? BottomText : TopText);
			valueTMP.text = m_Slider.value.ToString();
			if (base.ViewModel.ValueColor.HasValue)
			{
				Color defaultValueColor = valueTMP.color;
				Disposable.Create(delegate
				{
					valueTMP.color = defaultValueColor;
				}).AddTo(this);
				valueTMP.color = base.ViewModel.ValueColor.Value;
			}
		}
		TMP_Text textTMP = (base.ViewModel.IsValueOnBottom ? TopText : BottomText);
		textTMP.SetText(base.ViewModel.Text);
		if (base.ViewModel.TextColor.HasValue)
		{
			Color defaultTextColor = textTMP.color;
			Disposable.Create(delegate
			{
				textTMP.color = defaultTextColor;
			}).AddTo(this);
			textTMP.color = base.ViewModel.TextColor.Value;
		}
		Image.gameObject.SetActive(base.ViewModel.Sprite != null);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
