using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class OpenedCaseAnnotationsView : View<OpenedCaseAnnotationsVM>
{
	[SerializeField]
	private TMP_Text m_AnnotationsTitle;

	[Header("Descriptions")]
	[SerializeField]
	private TMP_Text m_NewEntityDescription;

	[SerializeField]
	private TMP_Text m_ConfirmedConclusionDescription;

	[SerializeField]
	private TMP_Text m_RefutedConclusionDescription;

	protected override void OnBind()
	{
		UIDetectiveJournal detectiveJournal = UIStrings.Instance.DetectiveJournal;
		m_AnnotationsTitle.text = detectiveJournal.AnnotationsTitle.Text;
		m_NewEntityDescription.text = detectiveJournal.NewEntityAnnotationDescription.Text;
		m_ConfirmedConclusionDescription.text = detectiveJournal.ConfirmedConclusionAnnotationDescription.Text;
		m_RefutedConclusionDescription.text = detectiveJournal.RefutedConclusionAnnotationDescription.Text;
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
