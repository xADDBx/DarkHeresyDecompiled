using Code.View.UI.Helpers;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class NonStackEntityView : View<BrickNonStackVM.NonStackEntity>
{
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private Image m_Icon;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		base.OnBind();
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title).AddTo(this);
		}
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.text = base.ViewModel.Name;
		m_TextHelper.UpdateTextSize();
	}
}
