using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AnalysisButtonView : View<DetectiveStudyVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private TMP_Text m_ButtonLabel;

	protected override void OnBind()
	{
		m_ButtonLabel.text = UIStrings.Instance.DetectiveJournal.ToStudiesLabel.Text;
		m_Description.text = base.ViewModel.StudyGroup?.StudyName;
		m_Button.OnLeftClickAsObservable().Subscribe(base.ViewModel.OnStudyClick).AddTo(this);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
