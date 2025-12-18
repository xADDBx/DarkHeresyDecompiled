using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseCardInfoEntityView : View<CaseCardInfoEntityVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Question;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	protected override void OnBind()
	{
		m_Question.text = base.ViewModel.Text;
		m_StateSelectable.SetActiveLayer(base.ViewModel.State.ToString());
	}
}
