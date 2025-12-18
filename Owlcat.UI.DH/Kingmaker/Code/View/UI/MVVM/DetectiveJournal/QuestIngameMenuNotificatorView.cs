using System;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class QuestIngameMenuNotificatorView : View<QuestIngameMenuNotificatorVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private MoveAnimator m_MoveAnimator;

	private IDisposable m_HintDisposable;

	protected override void OnBind()
	{
		base.ViewModel.IsNew.CombineLatest(base.ViewModel.IsInCombat, (bool isNew, bool inCombat) => new { isNew, inCombat }).Subscribe(value =>
		{
			SetState(value.isNew && !value.inCombat);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.OpenQuestJournalAtFirstNew();
		}).AddTo(this);
	}

	private void SetState(bool state)
	{
		m_Button.gameObject.SetActive(state);
		if (state)
		{
			Appear();
		}
		else
		{
			Disappear();
		}
	}

	private void Appear()
	{
		m_FadeAnimator.AppearAnimation();
		m_MoveAnimator.AppearAnimation();
	}

	private void Disappear()
	{
		m_FadeAnimator.DisappearAnimation();
		m_MoveAnimator.DisappearAnimation();
	}
}
