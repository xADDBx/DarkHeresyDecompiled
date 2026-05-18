using Code.View.UI.Helpers;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class MultipleTextWidget : View<MultipleTextData>
{
	[Header("Elements")]
	[SerializeField]
	private TextWithParent m_Text;

	[SerializeField]
	private LayoutGroup m_LayoutGroup;

	[ShowIf("m_HasIcon")]
	[SerializeField]
	private Image m_Icon;

	[Header("Values")]
	[SerializeField]
	private bool m_HasIcon;

	private AccessibilityTextHelper m_TextHelper;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Text.Text).AddTo(this);
		}
		if ((object)m_LayoutGroup == null)
		{
			m_LayoutGroup = m_Text.Container.EnsureComponent<LayoutGroup>();
		}
		m_Text.Text.SetText(base.ViewModel.Text.Text);
		m_LayoutGroup.childAlignment = base.ViewModel.Alignment;
		if (m_HasIcon)
		{
			m_Icon.sprite = base.ViewModel.Icon;
		}
	}
}
