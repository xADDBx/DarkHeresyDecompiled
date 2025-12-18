using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.Events;
using Kingmaker.Utility.DotNetExtensions;
using ObservableCollections;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class AnswerTierChangeVM : ViewModel, IAnswerTierViewed, ISubscriber
{
	public readonly BlueprintCase BlueprintCase;

	public ObservableList<BlueprintCaseAnswer> UpgradedAnswers = new ObservableList<BlueprintCaseAnswer>();

	private readonly Dictionary<BlueprintCaseAnswer, int> m_AnswerToTier = new Dictionary<BlueprintCaseAnswer, int>();

	public AnswerTierChangeVM(BlueprintCase blueprintCase)
	{
		BlueprintCase = blueprintCase;
		if (blueprintCase == null)
		{
			return;
		}
		foreach (BlueprintCaseAnswer item in UIUtilityDetective.GetAnswersWithTier(blueprintCase))
		{
			if (Game.Instance.DetectiveSystem.TryGetAnswerDegree(item, out var degree))
			{
				m_AnswerToTier.TryAdd(item, degree);
			}
		}
		EventBus.Subscribe(this).AddTo(this);
	}

	public void PopToast()
	{
		if (UpgradedAnswers.Count > 0)
		{
			BlueprintCaseAnswer answer = UpgradedAnswers[0];
			UpgradedAnswers.RemoveAt(0);
			EventBus.RaiseEvent(delegate(IAnswerTierViewed h)
			{
				h.HandleAnswerTierViewed(answer);
			});
		}
	}

	public void UpdateAnswers()
	{
		IEnumerable<BlueprintCaseAnswer> answersWithTier = UIUtilityDetective.GetAnswersWithTier(BlueprintCase);
		foreach (BlueprintCaseAnswer answer in answersWithTier)
		{
			m_AnswerToTier.TryAdd(answer, -1);
			if (Game.Instance.DetectiveSystem.TryGetAnswerDegree(answer, out var tier) && (!m_AnswerToTier.TryGetValue(answer, out var oldTier) || oldTier != tier))
			{
				if (UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ShowTierToastOnUpgrade) && tier > oldTier)
				{
					UpgradedAnswers.Add(answer);
				}
				if (UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ShowTierToastOnDowngrade) && tier < oldTier)
				{
					UpgradedAnswers.Add(answer);
				}
				EventBus.RaiseEvent(delegate(IAnswerTierChanged h)
				{
					h.HandleAnswerTierChanged(answer, oldTier, tier);
				});
				m_AnswerToTier[answer] = tier;
			}
		}
		foreach (KeyValuePair<BlueprintCaseAnswer, int> item in m_AnswerToTier.ToList())
		{
			var (key, oldTier) = (KeyValuePair<BlueprintCaseAnswer, int>)(ref item);
			if (!answersWithTier.Contains(key))
			{
				m_AnswerToTier[key] = -1;
				EventBus.RaiseEvent(delegate(IAnswerTierChanged h)
				{
					h.HandleAnswerTierChanged(key, oldTier, -1);
				});
				UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.ForgetAnswerTier(key);
			}
		}
	}

	public void ResetAnswers()
	{
		foreach (BlueprintCaseAnswer answer in UIUtilityDetective.GetAnswersWithTier(BlueprintCase))
		{
			Game.Instance.DetectiveSystem.TryGetAnswerDegree(answer, out var degree);
			if (!m_AnswerToTier.TryAdd(answer, degree))
			{
				m_AnswerToTier[answer] = degree;
			}
			UpgradedAnswers.RemoveAll((BlueprintCaseAnswer a) => a.Equals(answer));
		}
	}

	public void HandleAnswerTierViewed(BlueprintCaseAnswer answer)
	{
		UpgradedAnswers.RemoveAll((BlueprintCaseAnswer a) => a.Equals(answer));
	}
}
