using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionFeatureLevelView : View<ChargenProgressionFeatureLevelVM>
{
	[SerializeField]
	private GameObject m_Received;

	[SerializeField]
	private GameObject m_NotReceived;

	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private TextMeshProUGUI m_Acronym;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private TalentGroupView m_TalentIconView;

	protected override void OnBind()
	{
		base.OnBind();
		if (base.ViewModel.Tooltip != null)
		{
			this.SetTooltip(base.ViewModel.Tooltip).AddTo(this);
		}
		if (string.IsNullOrEmpty(base.ViewModel.Label) && string.IsNullOrEmpty(base.ViewModel.Acronym) && base.ViewModel.Icon == null)
		{
			m_NotReceived.SetActive(value: true);
			m_Received.SetActive(value: false);
			return;
		}
		m_Received.SetActive(value: true);
		m_NotReceived.SetActive(value: false);
		m_Icon?.transform.parent.gameObject.SetActive(value: false);
		m_Acronym?.transform.parent.gameObject.SetActive(value: false);
		m_Label?.transform.parent.gameObject.SetActive(value: false);
		m_TalentIconView?.transform.parent.gameObject.SetActive(value: false);
		if (m_TalentIconView != null && base.ViewModel.TalentIconInfo != null)
		{
			m_TalentIconView.transform.parent.gameObject.SetActive(value: true);
			m_TalentIconView.SetupView(base.ViewModel.TalentIconInfo);
			if (m_Acronym != null)
			{
				m_Acronym.text = base.ViewModel.Acronym;
			}
		}
		else if (m_Icon != null && base.ViewModel.Icon != null)
		{
			m_Icon.transform.parent.gameObject.SetActive(value: true);
			m_Icon.sprite = base.ViewModel.Icon;
		}
		else if (m_Acronym != null && !string.IsNullOrEmpty(base.ViewModel.Acronym))
		{
			m_Acronym.transform.parent.gameObject.SetActive(value: true);
			m_Acronym.text = base.ViewModel.Acronym;
		}
		else if (m_Label != null && !string.IsNullOrEmpty(base.ViewModel.Label))
		{
			m_Label.transform.parent.gameObject.SetActive(value: true);
			m_Label.text = base.ViewModel.Label;
		}
	}
}
