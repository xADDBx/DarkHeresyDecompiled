using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickColonyProjectProgressView : TooltipBaseBrickView<TooltipBrickColonyProjectProgressVM>
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TextMeshProUGUI m_Title;

	[SerializeField]
	private float m_DefaultFontSize = 18f;

	[SerializeField]
	private float m_DefaultConsoleFontSize = 18f;

	protected override void OnBind()
	{
		base.OnBind();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Label;
		m_Title.fontSize = (Game.Instance.IsControllerMouse ? m_DefaultFontSize : m_DefaultConsoleFontSize) * m_FontMultiplier;
	}
}
