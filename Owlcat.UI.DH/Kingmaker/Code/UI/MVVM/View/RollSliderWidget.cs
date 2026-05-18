using Kingmaker.Code.View.Bridge.Data;
using Owlcat.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class RollSliderWidget : View<(int, int?)>
{
	[Header("Slider")]
	[SerializeField]
	private Slider m_SufficientValueSlider;

	[SerializeField]
	private Slider m_CurrentValueSlider;

	[SerializeField]
	private TMP_Text m_ChanceValueText;

	[SerializeField]
	private Image m_ResultSignImage;

	[SerializeField]
	private Image m_CurrentHandleImage;

	[Header("Selectables")]
	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private TextStyle m_BlueTextStyle;

	[SerializeField]
	private TextStyle m_OrangeTextStyle;

	protected override void OnBind()
	{
		var (num, num2) = base.ViewModel;
		m_CurrentValueSlider.gameObject.SetActive(num2.HasValue);
		m_ResultSignImage.gameObject.SetActive(num2.HasValue);
		m_SufficientValueSlider.value = num;
		m_CurrentValueSlider.value = num2.GetValueOrDefault();
		string text = ((num2 == num) ? "=" : ((num2 < num) ? "<" : ">"));
		string text2 = ((!num2.HasValue) ? $"<style={m_OrangeTextStyle.Style.name}>{num}%</style>" : $"<style={m_BlueTextStyle.Style.name}>{num2.Value}</style> <style={m_OrangeTextStyle.Style.name}>{text} {num}%</style>");
		m_ChanceValueText.text = text2;
		bool flag = num2 <= num;
		m_StateSelectable.SetActiveLayer(flag ? "Success" : "Failed");
	}
}
