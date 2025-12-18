using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.MVVM.DetectiveJournal;
using Kingmaker.UI.Common.Animations;
using ObservableCollections;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Code.View.UI.MVVM.DetectiveJournal.CaseCard;

public class AnswersListView : View<AnswersListVM>
{
	[Header("Elements")]
	[SerializeField]
	private OwlcatMultiButton m_ShowHideButton;

	[SerializeField]
	private OwlcatMultiSelectable m_ShowHideSelectable;

	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private WidgetList m_Container;

	[SerializeField]
	private MoveAnimator m_Animator;

	[Header("Views")]
	[SerializeField]
	private SimpleAnswerView m_AnswerPrefab;

	private bool m_IsVisible;

	protected override void OnBind()
	{
		m_Title.text = UIStrings.Instance.DetectiveJournal.AnswersListTitle;
		ObservableSubscribeExtensions.Subscribe(m_ShowHideButton.OnLeftClickAsObservable(), delegate
		{
			ToggleVisibility();
		}).AddTo(this);
		base.ViewModel.Answers.ObserveCountChanged().Subscribe(delegate
		{
			DrawAnswers();
		}).AddTo(this);
		DrawAnswers();
	}

	private void DrawAnswers()
	{
		m_Container.Clear();
		m_Container.DrawEntries(base.ViewModel.Answers, m_AnswerPrefab).AddTo(this);
		UpdateExpandedState();
	}

	private void ToggleVisibility()
	{
		m_IsVisible = !m_IsVisible;
		if (m_IsVisible)
		{
			m_Animator.AppearAnimation();
		}
		else
		{
			m_Animator.DisappearAnimation();
		}
		m_ShowHideSelectable.SetActiveLayer(m_IsVisible ? 1 : 0);
		UpdateExpandedState();
	}

	private void UpdateExpandedState()
	{
		m_Container.Entries.ForEach(delegate(IBindable e)
		{
			(e as SimpleAnswerView)?.SetExpandState(m_IsVisible);
		});
	}
}
