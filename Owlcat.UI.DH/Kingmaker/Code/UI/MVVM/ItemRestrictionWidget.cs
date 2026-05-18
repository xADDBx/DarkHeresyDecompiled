using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ItemRestrictionWidget : View<string>
{
	[SerializeField]
	private TMP_Text m_Text;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		m_Text.text = base.ViewModel;
		m_TextHelper.UpdateTextSize();
	}
}
