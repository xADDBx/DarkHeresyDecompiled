using Code.View.UI.Helpers;
using Owlcat.Runtime.Core.Utility;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickIconAndNameView : BrickBaseView<BrickIconAndNameVM>
{
	[SerializeField]
	protected Image m_Icon;

	[SerializeField]
	protected TMP_Text m_Title;

	[SerializeField]
	private GameObject m_Frame;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Title).AddTo(this);
		}
		base.OnBind();
		m_Icon.sprite = base.ViewModel.Icon;
		m_Title.SetText(base.ViewModel.Text);
		m_Frame.Or(null)?.SetActive(base.ViewModel.Frame);
		this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		m_TextHelper.UpdateTextSize();
	}
}
