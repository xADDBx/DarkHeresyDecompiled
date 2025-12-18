using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Gameplay.Components;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class ClueInformationBaseView : View<BlueprintClue>
{
	[Header("Elements")]
	[SerializeField]
	private Image m_ClueIcon;

	[SerializeField]
	private TMP_Text m_ClueName;

	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private TMP_Text m_PotentialHypothesisTitle;

	[SerializeField]
	private OwlcatMultiSelectable m_AnswerStateSelectable;

	[SerializeField]
	private WidgetList m_AnswersContainer;

	[Header("Views")]
	[SerializeField]
	private ClueInformationDecorBaseView m_DecorView;

	[SerializeField]
	private CaseEntitySourceView m_SourceView;

	[SerializeField]
	private AnswerInfoView m_AnswerPrefab;

	[SerializeField]
	private ClueInfoNotesView m_NotesView;

	[Header("Values")]
	[field: SerializeField]
	public bool IsPaperInfo { get; private set; }

	protected override void OnBind()
	{
		ClueUIData uIData = base.ViewModel.GetUIData();
		m_ClueIcon.sprite = uIData.Icon;
		m_ClueName.text = uIData.Name;
		m_Description.text = uIData.Description;
		BlueprintCase blueprint = base.ViewModel.ParentCase.Blueprint;
		bool flag = blueprint.IsClosed() && Game.Instance.DetectiveSystem.GetCaseAnswer(blueprint)?.Answer.RelatedItem == base.ViewModel;
		m_PotentialHypothesisTitle.text = (flag ? UIStrings.Instance.DetectiveJournal.SelectedHypothesisTitle.Text : UIStrings.Instance.DetectiveJournal.PotentialHypothesisTitle.Text);
		m_DecorView.Bind(base.ViewModel);
		m_SourceView.Bind(new CaseEntitySourceVM(base.ViewModel, Game.Instance.DetectiveSystem.GetSource(base.ViewModel)));
		m_NotesView.Bind(new ClueInfoNotesVM(base.ViewModel));
		SetupAnswers();
		SetupTooltip();
	}

	protected override void OnUnbind()
	{
		m_DecorView.Unbind();
	}

	private void SetupTooltip()
	{
		if (base.ViewModel.TryGetComponent<ClueToItemLink>(out var component))
		{
			m_ClueIcon.SetTooltip(new TooltipTemplateItem(component.GetLastLinkedItem())).AddTo(this);
		}
	}

	private void SetupAnswers()
	{
		BlueprintCase blueprint = base.ViewModel.ParentCase.Blueprint;
		BlueprintCaseAnswer selectedAnswer = Game.Instance.DetectiveSystem.GetCaseAnswer(blueprint)?.Answer;
		bool num = !blueprint.IsOpen() && selectedAnswer?.RelatedItem == base.ViewModel;
		List<BlueprintCaseAnswer> list = UIUtilityDetective.GetAnswersFor(base.ViewModel).ToList();
		list.RemoveAll((BlueprintCaseAnswer a) => !Game.Instance.DetectiveSystem.TryGetAnswerDegree(a, out var degree) || degree < 0);
		if (num)
		{
			list.RemoveAll((BlueprintCaseAnswer a) => selectedAnswer != a);
		}
		if (num)
		{
			m_AnswerStateSelectable.SetActiveLayer((list.Count > 0) ? 2 : 0);
		}
		else
		{
			m_AnswerStateSelectable.SetActiveLayer((list.Count > 0) ? 1 : 0);
		}
		m_AnswersContainer.DrawEntries(list, m_AnswerPrefab).AddTo(this);
		m_AnswersContainer.gameObject.SetActive(list.Count > 0);
	}
}
