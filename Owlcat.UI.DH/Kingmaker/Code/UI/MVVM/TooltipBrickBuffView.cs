using Owlcat.Runtime.Core.Utility;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TooltipBrickBuffView : TooltipBrickFeatureView
{
	[SerializeField]
	private TextMeshProUGUI m_Duration;

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
		(base.ViewModel as TooltipBrickBuffVM)?.Duration.Subscribe(delegate(string t)
		{
			m_Duration.text = t;
			if (t == string.Empty)
			{
				base.gameObject.SetActive(value: false);
			}
		}).AddTo(this);
		if (base.ViewModel is TooltipBrickBuffVM tooltipBrickBuffVM)
		{
			m_SourcePanel.Or(null)?.SetActive(!string.IsNullOrWhiteSpace(tooltipBrickBuffVM.SourceName));
			if (m_SourceName != null)
			{
				m_SourceName.text = tooltipBrickBuffVM.SourceName;
			}
			if (m_StackText != null)
			{
				m_StackText.text = tooltipBrickBuffVM.Stack;
			}
			m_DOTPanel.SetActive(tooltipBrickBuffVM.IsDOT);
			if (tooltipBrickBuffVM.IsDOT)
			{
				m_DOTDescription.text = tooltipBrickBuffVM.DOTDesc;
				m_DOTDamage.text = tooltipBrickBuffVM.DOTDamage;
			}
		}
	}
}
