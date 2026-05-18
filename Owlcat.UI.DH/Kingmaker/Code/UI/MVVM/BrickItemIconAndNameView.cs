using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickItemIconAndNameView : BrickBaseView<BrickItemIconAndNameVM>
{
	[Header("Elements")]
	[SerializeField]
	protected TMP_Text m_Title;

	[SerializeField]
	protected Image m_Icon;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title).AddTo(this);
		}
		base.OnBind();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Label;
		m_TextHelper.UpdateTextSize();
	}
}
