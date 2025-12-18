using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Models.Log.GameLogCntxt;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseCardVM : ViewModel
{
	public readonly BlueprintCase BlueprintCase;

	private readonly ReactiveProperty<CardState> m_CurrentState = new ReactiveProperty<CardState>();

	private readonly ReactiveProperty<int> m_NewItemsCount = new ReactiveProperty<int>();

	private readonly ObservableList<CaseCardInfoEntityVM> m_Questions = new ObservableList<CaseCardInfoEntityVM>();

	private readonly Action m_OnCardClick;

	public ReadOnlyReactiveProperty<CardState> CurrentState => m_CurrentState;

	public ReadOnlyReactiveProperty<int> NewItemsCount => m_NewItemsCount;

	public ObservableList<CaseCardInfoEntityVM> Questions => m_Questions;

	public CaseCardVM(BlueprintCase blueprintCase, Action<BlueprintCase> onCardClick = null)
	{
		CaseCardVM caseCardVM = this;
		BlueprintCase = blueprintCase;
		m_OnCardClick = delegate
		{
			onCardClick?.Invoke(caseCardVM.BlueprintCase);
		};
		UpdateState();
	}

	public void ClickOnCard()
	{
		m_OnCardClick?.Invoke();
	}

	private void UpdateState()
	{
		m_CurrentState.Value = GetState();
		UpdateQuestions();
		UpdateNewItemsCount();
	}

	private void UpdateQuestions()
	{
		m_Questions.Clear();
		switch (CurrentState.CurrentValue)
		{
		case CardState.Default:
		{
			CaseCardInfoEntityVM item = new CaseCardInfoEntityVM(Game.Instance.DetectiveSystem.GetActualCaseQuestion(BlueprintCase).Name, CardState.Default);
			m_Questions.Add(item);
			break;
		}
		case CardState.Closed:
		{
			BlueprintCaseAnswer blueprintCaseAnswer = Game.Instance.DetectiveSystem.GetCaseAnswer(BlueprintCase)?.Answer;
			using (GameLogContext.Scope)
			{
				GameLogContext.CaseAnswer = blueprintCaseAnswer;
				string text = UIStrings.Instance.DetectiveJournal.ClosedVerdict.Text;
				m_Questions.Add(new CaseCardInfoEntityVM(text, CardState.Closed));
				break;
			}
		}
		case CardState.Failed:
			m_Questions.Add(new CaseCardInfoEntityVM(Game.Instance.DetectiveSystem.GetActualCaseQuestion(BlueprintCase).Name, CardState.Failed));
			break;
		case CardState.Unknown:
			m_Questions.Add(new CaseCardInfoEntityVM(UIStrings.Instance.DetectiveJournal.UnknownCluesDescription, CardState.Unknown));
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	private void UpdateNewItemsCount()
	{
		if (BlueprintCase == null)
		{
			m_NewItemsCount.Value = UIUtilityDetective.GetUnknownClues().Where(UIUtilityDetective.ExaminedDetectiveData.ExaminedClues.IsEntityNew).Count();
			return;
		}
		if (BlueprintCase.IsClosed())
		{
			m_NewItemsCount.Value = 0;
			return;
		}
		IEnumerable<BlueprintClue> source = UIUtilityDetective.GetNewClues(BlueprintCase).Concat(UIUtilityDetective.GetCluesWithNewAddendums(BlueprintCase)).Concat(UIUtilityDetective.GetCluesWithNewStudies(BlueprintCase))
			.Concat(UIUtilityDetective.GetCluesWithNewConclusions(BlueprintCase));
		IEnumerable<BlueprintConclusion> source2 = UIUtilityDetective.GetNewConclusions(BlueprintCase).Concat(UIUtilityDetective.GetConclusionsWithNewConclusions(BlueprintCase));
		m_NewItemsCount.Value = source.Distinct().Count() + source2.Distinct().Count();
	}

	private CardState GetState()
	{
		if (BlueprintCase == null)
		{
			return CardState.Unknown;
		}
		if (!BlueprintCase.IsClosed())
		{
			return CardState.Default;
		}
		if (!BlueprintCase.IsFailed())
		{
			return CardState.Closed;
		}
		return CardState.Failed;
	}
}
