using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickIconAndTextWithCustomColorsView : TooltipBaseBrickView<TooltipBrickIconAndTextWithCustomColorsVM>
{
	[SerializeField]
	private TextMeshProUGUI m_StringValue;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private Image m_Background;

	[SerializeField]
	private float m_DefaultFontSize = 22f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 22f;

	protected override void OnBind()
	{
		base.OnBind();
		m_StringValue.text = base.ViewModel.StringValue;
		m_Icon.sprite = base.ViewModel.Icon;
		m_StringValue.color = base.ViewModel.StringValueColor;
		m_Icon.color = base.ViewModel.IconColor;
		m_Background.color = base.ViewModel.BackgroundColor;
		m_StringValue.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
	}
}
