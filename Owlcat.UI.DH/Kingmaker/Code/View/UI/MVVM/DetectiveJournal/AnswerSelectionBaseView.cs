using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AnswerSelectionBaseView : View<AnswerSelectionVM>
{
	[Header("Views")]
	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private ReportAnswerView m_SelectionView;

	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Question;

	protected override void OnBind()
	{
		m_Question.text = base.ViewModel.ReportContext.Question.Name.Text;
		m_WidgetList.DrawEntries(base.ViewModel.EntitiesCollection, m_SelectionView);
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}
}
