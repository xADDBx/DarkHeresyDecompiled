using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class BrickHintView : BrickBaseView<BrickHintVM>
{
	[SerializeField]
	private TMP_Text m_Text;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text).AddTo(this);
		}
		base.OnBind();
		m_Text.SetText(base.ViewModel.Text);
		m_TextHelper.UpdateTextSize();
	}
}
