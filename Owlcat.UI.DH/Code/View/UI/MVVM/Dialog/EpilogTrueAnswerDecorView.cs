using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Code.View.UI.MVVM.Dialog;

public class EpilogTrueAnswerDecorView : View<Unit>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_OfficialTitleReport;

	[SerializeField]
	private TMP_Text m_MainDepartmentLabel;

	[SerializeField]
	private TMP_Text m_ToDepartmentLabel;

	[SerializeField]
	private TMP_Text m_ArchiveLabel;

	[SerializeField]
	private TMP_Text m_NumberDecor;

	protected override void OnBind()
	{
		DetectiveJournalDecor detectiveDecor = UIStrings.Instance.DetectiveDecor;
		m_OfficialTitleReport.text = detectiveDecor.OfficialReportTitle.Text;
		m_MainDepartmentLabel.text = detectiveDecor.MainDepartmentTitle.Text;
		m_ToDepartmentLabel.text = detectiveDecor.ToDepartmentTitle.Text;
		m_ArchiveLabel.text = detectiveDecor.ArchiveName.Text;
	}
}
