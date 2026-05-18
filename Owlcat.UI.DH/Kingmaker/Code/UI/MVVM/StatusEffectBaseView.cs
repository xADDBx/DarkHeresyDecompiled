using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public abstract class StatusEffectBaseView : CharInfoFeatureSimpleBaseView
{
	[SerializeField]
	protected TextMeshProUGUI m_Duration;

	[Header("SourceBlock")]
	[SerializeField]
	private GameObject m_SourcePanel;

	[SerializeField]
	private TextMeshProUGUI m_SourceName;

	[SerializeField]
	private TextMeshProUGUI m_StackText;

	[Header("DOTBlock")]
	[SerializeField]
	private GameObject m_DOTPanel;

	[SerializeField]
	private TextMeshProUGUI m_DOTDescription;

	[SerializeField]
	private TextMeshProUGUI m_DOTDamage;

	protected override void OnBind()
	{
		base.OnBind();
		SetupDescription();
		m_TextHelper.AppendTexts(m_Duration);
		m_DOTPanel.SetActive(value: false);
	}

	private void SetupDescription()
	{
		if (!(m_Duration == null))
		{
			m_Duration.gameObject.SetActive(!string.IsNullOrEmpty(base.ViewModel.TimeLeft));
			m_Duration.text = base.ViewModel.TimeLeft;
			m_SourceName.text = base.ViewModel.SourceName;
			m_StackText.text = base.ViewModel.StacksText;
		}
	}
}
