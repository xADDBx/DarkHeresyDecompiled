using System;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Events;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class SimpleAnswerVM : ViewModel
{
	public readonly BlueprintCaseAnswer Answer;

	private readonly ReactiveProperty<string> m_AnswerDescription = new ReactiveProperty<string>();

	private readonly ReactiveProperty<bool> m_IsNew = new ReactiveProperty<bool>();

	private readonly Action m_OnAnswerClick;

	public ReadOnlyReactiveProperty<string> AnswerDescription => m_AnswerDescription;

	public ReadOnlyReactiveProperty<bool> IsNew => m_IsNew;

	public SimpleAnswerVM(BlueprintCaseAnswer answer, Action<BlueprintCaseAnswer> onAnswerClick)
	{
		Answer = answer;
		m_AnswerDescription.Value = UIUtilityDetective.GetAnswerDegreeDescription(answer).Text;
		m_OnAnswerClick = delegate
		{
			onAnswerClick?.Invoke(answer);
		};
		m_IsNew.Value = UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.IsEntityNew(answer);
	}

	public void OnAnswerClick()
	{
		m_OnAnswerClick?.Invoke();
	}

	public void MarkAsViewed()
	{
		EventBus.RaiseEvent(delegate(IAnswerTierViewed h)
		{
			h.HandleAnswerTierViewed(Answer);
		});
	}

	public void HandleAnswerTierViewed()
	{
		m_IsNew.Value = false;
		UIUtilityDetective.ExaminedDetectiveData.ExaminedAnswersData.AddExaminedAnswerIfNeeded(Answer);
	}

	public void HandleTierChanged()
	{
		m_IsNew.Value = true;
		m_AnswerDescription.Value = UIUtilityDetective.GetAnswerDegreeDescription(Answer).Text;
	}
}
