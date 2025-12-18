using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseCardDescriptionView : View<CaseCardVM>
{
	[Header("Elements")]
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private WidgetList m_QuestionsContainer;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[Header("Views")]
	[SerializeField]
	private CaseCardInfoEntityView InfoEntityPrefab;

	protected override void OnBind()
	{
		base.ViewModel.CurrentState.Subscribe(delegate(CardState value)
		{
			m_StateSelectable.SetActiveLayer(value.ToString());
		}).AddTo(this);
		m_Title.text = ((base.ViewModel.BlueprintCase == null) ? UIStrings.Instance.DetectiveJournal.UnknownCluesHeader.Text : base.ViewModel.BlueprintCase.Name.Text);
		base.ViewModel.Questions.ObserveAdd().Subscribe(delegate
		{
			DrawQuestions();
		}).AddTo(this);
		DrawQuestions();
	}

	private void DrawQuestions()
	{
		CaseCardInfoEntityVM[] datas = base.ViewModel.Questions.ToArray();
		m_QuestionsContainer.DrawEntries(datas, InfoEntityPrefab).AddTo(this);
	}
}
