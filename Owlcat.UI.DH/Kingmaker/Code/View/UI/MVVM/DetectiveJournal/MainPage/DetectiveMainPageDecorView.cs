using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal.MainPage;

public class DetectiveMainPageDecorView : View<Unit>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_ToggleViewLabel;

	[SerializeField]
	private TMP_Text m_EmptyCasesNoData;

	[SerializeField]
	private TMP_Text m_EmptyCasesCtA;

	protected override void OnBind()
	{
		m_Title.text = UIStrings.Instance.DetectiveJournal.CurrentCases.Text;
		m_ToggleViewLabel.text = UIStrings.Instance.DetectiveJournal.ToggleClosedCases.Text;
		m_EmptyCasesNoData.text = UIStrings.Instance.DetectiveJournal.NoCasesLabel.Text;
		m_EmptyCasesCtA.text = UIStrings.Instance.DetectiveJournal.NoCasesCallToAction.Text;
	}
}
