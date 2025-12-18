using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AnswersListVM : ViewModel, IAnswerTierChanged, ISubscriber, IAnswerTierViewed
{
	public readonly BlueprintCase BlueprintCase;

	public readonly ObservableList<SimpleAnswerVM> Answers = new ObservableList<SimpleAnswerVM>();

	private readonly Action<BlueprintCaseAnswer> m_OnAnswerClick;

	public AnswersListVM(BlueprintCase blueprintCase, Action<BlueprintCaseAnswer> onAnswerClick)
	{
		BlueprintCase = blueprintCase;
		m_OnAnswerClick = onAnswerClick;
		if (blueprintCase != null)
		{
			UpdateAnswers();
			EventBus.Subscribe(this).AddTo(this);
		}
	}

	private void UpdateAnswers()
	{
		IEnumerable<BlueprintCaseAnswer> answersWithTier = UIUtilityDetective.GetAnswersWithTier(BlueprintCase);
		foreach (BlueprintCaseAnswer caseAnswer in answersWithTier)
		{
			if (!Answers.Any((SimpleAnswerVM a) => a.Answer.Equals(caseAnswer)))
			{
				Answers.Add(new SimpleAnswerVM(caseAnswer, m_OnAnswerClick));
			}
		}
		foreach (SimpleAnswerVM item in Answers.ToList())
		{
			if (!answersWithTier.Contains(item.Answer))
			{
				Answers.Remove(item);
			}
		}
	}

	public void MarkAsViewed()
	{
		Answers.ForEach(delegate(SimpleAnswerVM a)
		{
			a.MarkAsViewed();
		});
	}

	public void HandleAnswerTierChanged(BlueprintCaseAnswer answer, int oldTier, int newTier)
	{
		SimpleAnswerVM simpleAnswerVM = Answers.FirstOrDefault((SimpleAnswerVM a) => a.Answer.Equals(answer));
		if (simpleAnswerVM != null && newTier >= 0)
		{
			simpleAnswerVM.HandleTierChanged();
		}
		else
		{
			UpdateAnswers();
		}
	}

	public void HandleAnswerTierViewed(BlueprintCaseAnswer answer)
	{
		Answers.FirstOrDefault((SimpleAnswerVM a) => a.Answer.Equals(answer))?.HandleAnswerTierViewed();
	}
}
