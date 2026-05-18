using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickTextSignatureValueView : BrickBaseView<BrickTextSignatureValueVM>
{
	[SerializeField]
	private TMP_Text m_Text;

	[SerializeField]
	private TMP_Text m_SignatureText;

	[SerializeField]
	private TMP_Text m_Value;

	protected override void OnBind()
	{
		m_TextHelper = new AccessibilityTextHelper(m_Text, m_SignatureText, m_Value).AddTo(this);
		m_Text.text = base.ViewModel.Text;
		m_SignatureText.text = base.ViewModel.SignatureText;
		m_Value.text = base.ViewModel.Value;
		m_TextHelper.UpdateTextSize();
	}
}
