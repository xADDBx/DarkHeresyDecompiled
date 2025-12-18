using Code.View.UI.Helpers;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class TooltipBrickTextSignatureValueView : TooltipBaseBrickView<TooltipBrickTextSignatureValueVM>
{
	[SerializeField]
	private TextMeshProUGUI m_Text;

	[SerializeField]
	private TextMeshProUGUI m_SignatureText;

	[SerializeField]
	private TextMeshProUGUI m_Value;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_Text, m_SignatureText, m_Value);
		m_Text.text = base.ViewModel.Text;
		m_SignatureText.text = base.ViewModel.SignatureText;
		m_Value.text = base.ViewModel.Value;
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}
}
