using System.Collections.Generic;
using Kingmaker.Gameplay.Features.Encounter;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.CombatNotifications.CombatObjectives;

public class CombatObjectivesView : View<CombatObjectivesVM>
{
	[SerializeField]
	private TMP_Text m_HeaderText;

	[SerializeField]
	private CombatObjectiveView m_WidgetPrefab;

	[SerializeField]
	private Transform m_WidgetContainer;

	[SerializeField]
	private OwlcatMultiButton m_FoldButton;

	[SerializeField]
	private OwlcatMultiButton m_UnfoldButton;

	[SerializeField]
	private GameObject m_FoldedState;

	[SerializeField]
	private GameObject m_UnfoldedState;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private RectTransform m_HintTransform;

	[SerializeField]
	private FadeAnimator m_HintFadeAnimator;

	[SerializeField]
	private TMP_Text m_HintText;

	private readonly List<CombatObjectiveView> m_ActiveWidgets = new List<CombatObjectiveView>();

	private bool m_HighlightObjectivesRequested;

	protected override void OnBind()
	{
		m_HeaderText.text = base.ViewModel.TitleText;
		m_HighlightObjectivesRequested = true;
		if (base.ViewModel.ObjectiveVMs.Count < 1)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		foreach (CombatObjectiveVM objectiveVM in base.ViewModel.ObjectiveVMs)
		{
			CombatObjectiveView widget = WidgetFactory.GetWidget(m_WidgetPrefab);
			widget.transform.SetParent(m_WidgetContainer, worldPositionStays: false);
			widget.Bind(objectiveVM);
			m_ActiveWidgets.Add(widget);
		}
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ObjectiveActivated, delegate
		{
			Unfold();
		}).AddTo(this);
		base.ViewModel.IsVisible.Subscribe(SetVisible).AddTo(this);
		base.ViewModel.ToggleHint.Subscribe(HandleHint).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_FoldButton.OnLeftClickAsObservable(), delegate
		{
			Fold();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_UnfoldButton.OnLeftClickAsObservable(), delegate
		{
			Unfold();
		}).AddTo(this);
		m_FoldButton.SetHint(base.ViewModel.HideDirectivesHintText).AddTo(this);
		m_UnfoldButton.SetHint(base.ViewModel.ShowDirectivesHintText).AddTo(this);
		Unfold();
		m_HintFadeAnimator.DisappearInstant();
	}

	protected override void OnUnbind()
	{
		foreach (CombatObjectiveView activeWidget in m_ActiveWidgets)
		{
			WidgetFactory.DisposeWidget(activeWidget);
		}
		m_ActiveWidgets.Clear();
	}

	private void HandleHint((bool show, string text, RectTransform anchor) hintParams)
	{
		m_HintFadeAnimator.PlayAnimation(hintParams.show);
		if (hintParams.show)
		{
			m_HintText.SetText(hintParams.text);
			Vector3 position = m_HintTransform.position;
			position.y = hintParams.anchor.position.y;
			m_HintTransform.position = position;
		}
	}

	private void Fold()
	{
		m_FoldedState.SetActive(value: true);
		m_UnfoldedState.SetActive(value: false);
	}

	private void Unfold()
	{
		m_FoldedState.SetActive(value: false);
		m_UnfoldedState.SetActive(value: true);
	}

	private void SetVisible(bool isVisible)
	{
		if (isVisible)
		{
			HighlightObjectives();
		}
		m_FadeAnimator.PlayAnimation(isVisible);
	}

	private void HighlightObjectives()
	{
		if (!m_HighlightObjectivesRequested)
		{
			return;
		}
		m_HighlightObjectivesRequested = false;
		foreach (CombatObjectiveVM objectiveVM in base.ViewModel.ObjectiveVMs)
		{
			if (objectiveVM.State.CurrentValue == EncounterObjectiveState.Active)
			{
				objectiveVM.SetHighlighted(highlighted: true);
			}
		}
	}
}
