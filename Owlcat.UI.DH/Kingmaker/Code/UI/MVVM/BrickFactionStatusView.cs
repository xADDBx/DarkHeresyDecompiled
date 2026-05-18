using Code.View.UI.Helpers;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickFactionStatusView : BrickBaseView<BrickFactionStatusVM>
{
	[Header("Elements")]
	[SerializeField]
	protected TMP_Text m_Label;

	[SerializeField]
	protected TMP_Text m_Status;

	[SerializeField]
	protected Image m_Icon;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_Label, m_Status).AddTo(this);
		}
		base.OnBind();
		m_Label.SetText(base.ViewModel.Label);
		m_Status.SetText(base.ViewModel.Status);
		m_Icon.sprite = base.ViewModel.Icon;
		m_Icon.gameObject.SetActive(base.ViewModel.Icon != null);
	}
}
