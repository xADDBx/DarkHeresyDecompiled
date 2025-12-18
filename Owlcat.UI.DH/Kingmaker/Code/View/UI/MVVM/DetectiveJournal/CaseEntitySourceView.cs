using Kingmaker.Code.UI.MVVM;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseEntitySourceView : View<CaseEntitySourceVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_SourceName;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected override void OnBind()
	{
		if (base.ViewModel.Source == null)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		m_SourceName.text = base.ViewModel.Source.SourceLabel;
		m_StateSelectable.SetTooltip(base.ViewModel.Source.Tooltip);
		m_StateSelectable.SetActiveLayer(base.ViewModel.Source.IssueType.ToString());
		m_StateSelectable.gameObject.SetActive(value: true);
	}
}
