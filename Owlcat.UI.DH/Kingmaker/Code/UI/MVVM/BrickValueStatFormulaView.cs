using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickValueStatFormulaView : BrickBaseView<BrickValueStatFormulaVM>
{
	[SerializeField]
	private TMP_Text m_Value;

	[SerializeField]
	private TMP_Text m_Symbol;

	[SerializeField]
	private TMP_Text m_Title;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Value, m_Symbol, m_Title).AddTo(this);
		}
		base.OnBind();
		m_Value.text = base.ViewModel.Value;
		m_Symbol.text = base.ViewModel.Symbol;
		m_Title.text = base.ViewModel.Name;
		m_TextHelper.UpdateTextSize();
	}
}
