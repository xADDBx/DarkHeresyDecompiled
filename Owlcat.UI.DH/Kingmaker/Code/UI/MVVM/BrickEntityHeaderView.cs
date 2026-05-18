using Code.View.UI.Helpers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BrickEntityHeaderView : BrickBaseView<BrickEntityHeaderVM>
{
	[Header("Title")]
	[SerializeField]
	private TMP_Text m_MainTitle;

	[Header("Left Side")]
	[SerializeField]
	private TMP_Text m_ItemType;

	[SerializeField]
	private TMP_Text m_ItemLabel;

	[SerializeField]
	private TMP_Text m_ItemSubtitle;

	[SerializeField]
	private GameObject m_UpgradeItemIndicator;

	[Header("Right Side")]
	[SerializeField]
	private GameObject m_ImageContainer;

	[SerializeField]
	private Image m_Image;

	protected override void OnBind()
	{
		if (m_TextHelper == null)
		{
			m_TextHelper = new AccessibilityTextHelper(m_MainTitle, m_ItemType, m_ItemLabel, m_ItemSubtitle);
		}
		base.OnBind();
		m_MainTitle.text = base.ViewModel.MainTitle;
		m_ImageContainer.SetActive(base.ViewModel.Image != null);
		m_Image.sprite = base.ViewModel.Image;
		SetText(m_ItemType, base.ViewModel.Title);
		SetText(m_ItemLabel, base.ViewModel.LeftLabel);
		SetText(m_ItemSubtitle, base.ViewModel.RightLabel);
		if ((bool)m_UpgradeItemIndicator)
		{
			m_UpgradeItemIndicator.SetActive(base.ViewModel.HasUpgrade);
		}
		m_TextHelper.UpdateTextSize();
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		m_TextHelper.Dispose();
	}

	private void SetText(TMP_Text textField, string text)
	{
		textField.gameObject.SetActive(!string.IsNullOrEmpty(text));
		textField.text = text;
	}
}
