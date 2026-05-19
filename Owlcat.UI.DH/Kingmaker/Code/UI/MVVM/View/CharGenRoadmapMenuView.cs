using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenRoadmapMenuView : View<SelectionGroupRadioVM<CharGenPhaseBaseVM>>
{
	private const float SelectorLevelOffset = 12f;

	[SerializeField]
	protected OwlcatMultiSelectable m_BackgroundFrame;

	[SerializeField]
	private RectTransform m_FrameStart;

	[SerializeField]
	private RectTransform m_FrameEnd;

	[SerializeField]
	private ScrollRectExtended m_ScrollRectExtended;

	[SerializeField]
	private Transform m_PhaseRoadmapRoot;

	[SerializeField]
	private CharGenCommonPhaseRoadmapView m_PhaseRoadmapViewPrefab;

	[SerializeField]
	private RectTransform m_Selector;

	[SerializeField]
	private RectTransform m_Glow;

	[SerializeField]
	private float m_AnimationDuration = 0.55f;

	[Header("Scroll")]
	[SerializeField]
	private OwlcatMultiButton m_LeftScrollButton;

	[SerializeField]
	private OwlcatMultiButton m_RightScrollButton;

	[SerializeField]
	private float m_ScrollDelta;

	private IDisposable m_DelayedSelectorMoveDisposable;

	private ICharGenPhaseRoadmapView m_SelectedView;

	private bool m_SelectorSoundPlaying;

	private CharGenPhaseBaseVM m_PrevEntity;

	private List<CharGenCommonPhaseRoadmapView> m_PhaseViews = new List<CharGenCommonPhaseRoadmapView>();

	protected override void OnBind()
	{
		ClearRoadmapPhaseViews();
		foreach (CharGenPhaseBaseVM item in base.ViewModel.EntitiesCollection)
		{
			CreateRoadmapPhaseView(item);
		}
		base.ViewModel.EntitiesCollection.ObserveAdd().Subscribe(CreateRoadmapPhaseView).AddTo(this);
		base.ViewModel.EntitiesCollection.ObserveRemove().Subscribe(RemoveRoadmapPhaseView).AddTo(this);
		base.ViewModel.EntitiesCollection.ObserveReset().Subscribe(ClearRoadmapPhaseViews).AddTo(this);
		base.ViewModel.EntitiesCollection.ObserveMove().Subscribe(delegate
		{
			DelayedMoveSelector(base.ViewModel.SelectedEntity.Value);
		}).AddTo(this);
		base.ViewModel.SelectedEntity.Subscribe(DelayedMoveSelector).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdateScrollButtonsInteractable();
		}).AddTo(this);
		OwlcatR3UnitExtensions.Subscribe(m_LeftScrollButton.OnLeftClickAsObservable(), delegate
		{
			SmoothScrollBy(m_ScrollDelta);
		}).AddTo(this);
		OwlcatR3UnitExtensions.Subscribe(m_RightScrollButton.OnLeftClickAsObservable(), delegate
		{
			SmoothScrollBy(0f - m_ScrollDelta);
		}).AddTo(this);
		m_ScrollRectExtended.OnDragAsObservable().Subscribe(delegate
		{
			m_DelayedSelectorMoveDisposable?.Dispose();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(2), delegate
		{
			UpdateBackgroundFrameSize();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		KillSelectorTween();
		ShutUpSelector();
		ClearRoadmapPhaseViews();
	}

	private void RemoveRoadmapPhaseView(CollectionRemoveEvent<CharGenPhaseBaseVM> removeEvent)
	{
		foreach (CharGenCommonPhaseRoadmapView item in m_PhaseViews.Where((CharGenCommonPhaseRoadmapView v) => v.ViewModel == removeEvent.Value || v.ViewModel == null).ToList())
		{
			m_PhaseViews.Remove(item);
			WidgetPool.Release(item);
		}
	}

	private void DelayedMoveSelector(CharGenPhaseBaseVM selectedEntity)
	{
		m_SelectedView = GetRoadmapPhaseView(selectedEntity);
		if (m_SelectedView == null)
		{
			return;
		}
		m_DelayedSelectorMoveDisposable?.Dispose();
		m_DelayedSelectorMoveDisposable = ObservableSubscribeExtensions.Subscribe(Observable.TimerFrame(2), delegate
		{
			m_ScrollRectExtended.EnsureVisibleHorizontal(m_SelectedView.ViewRectTransform, 160f);
			float x = m_SelectedView.ViewRectTransform.position.x;
			m_Selector.DOMoveX(x, m_AnimationDuration).OnStart(delegate
			{
				if (m_PrevEntity != selectedEntity)
				{
					SystemSounds.Instance.Selector.Start.Play();
					SystemSounds.Instance.Selector.LoopStart.Play();
					m_SelectorSoundPlaying = true;
					m_PrevEntity = selectedEntity;
				}
			}).OnComplete(ShutUpSelector)
				.OnKill(ShutUpSelector)
				.SetUpdate(isIndependentUpdate: true);
		}).AddTo(this);
	}

	private void MoveSelectorImmediately()
	{
		m_SelectedView = GetRoadmapPhaseView(base.ViewModel.SelectedEntity.CurrentValue);
		if (m_SelectedView != null)
		{
			float x = m_SelectedView.ViewRectTransform.position.x;
			m_Selector.position = new Vector3(x, m_Selector.position.y, m_Selector.position.z);
			m_Glow.position = new Vector3(x, m_Glow.position.y, m_Glow.position.z);
		}
	}

	public void SetBackgroundFrameState(bool isCustom)
	{
		m_BackgroundFrame.SetActiveLayer(isCustom ? "Custom" : "Pregen");
	}

	private void UpdateBackgroundFrameSize()
	{
		RectTransform rectTransform = m_BackgroundFrame.transform as RectTransform;
		float x = m_FrameStart.localPosition.x;
		rectTransform.localPosition = new Vector3(x, rectTransform.localPosition.y, 0f);
		float x2 = m_FrameEnd.localPosition.x - x;
		rectTransform.sizeDelta = new Vector2(x2, rectTransform.sizeDelta.y);
	}

	public void ShutUpSelector()
	{
		if (m_SelectorSoundPlaying)
		{
			SystemSounds.Instance.Selector.Stop.Play();
		}
		UISounds.Instance.Play(SystemSounds.Instance.Selector.LoopStop, isButton: false, playAnyway: true);
		m_SelectorSoundPlaying = false;
	}

	public void KillSelectorTween()
	{
		m_Selector.DOKill();
		DOTween.Kill(m_Selector);
	}

	private void CreateRoadmapPhaseView(CollectionAddEvent<CharGenPhaseBaseVM> addEvent)
	{
		CreateRoadmapPhaseView(addEvent.Value);
	}

	private void ClearRoadmapPhaseViews()
	{
		foreach (CharGenCommonPhaseRoadmapView phaseView in m_PhaseViews)
		{
			WidgetPool.Release(phaseView);
		}
		m_PhaseViews.Clear();
	}

	private void CreateRoadmapPhaseView(CharGenPhaseBaseVM phaseVM)
	{
		CharGenCommonPhaseRoadmapView charGenCommonPhaseRoadmapView = WidgetPool.Retain(m_PhaseRoadmapViewPrefab, m_PhaseRoadmapRoot);
		charGenCommonPhaseRoadmapView.Bind(phaseVM);
		m_PhaseViews.Add(charGenCommonPhaseRoadmapView);
		charGenCommonPhaseRoadmapView.transform.SetSiblingIndex(base.ViewModel.EntitiesCollection.IndexOf(phaseVM));
	}

	private ICharGenPhaseRoadmapView GetRoadmapPhaseView(CharGenPhaseBaseVM phaseVM)
	{
		return m_PhaseViews.FirstOrDefault((CharGenCommonPhaseRoadmapView view) => view.ViewModel == phaseVM);
	}

	private void UpdateScrollButtonsInteractable()
	{
		bool flag = m_ScrollRectExtended.content.rect.width - m_ScrollRectExtended.viewport.rect.width > 0.1f;
		bool flag2 = m_ScrollRectExtended.horizontalNormalizedPosition > 0.001f;
		bool flag3 = m_ScrollRectExtended.horizontalNormalizedPosition < 0.999f;
		m_LeftScrollButton.Interactable = flag && flag2;
		m_RightScrollButton.Interactable = flag && flag3;
		if (m_DelayedSelectorMoveDisposable == null)
		{
			MoveSelectorImmediately();
		}
	}

	private void SmoothScrollBy(float delta)
	{
		PointerEventData data = new PointerEventData(EventSystem.current)
		{
			scrollDelta = new Vector2(delta, 0f)
		};
		m_ScrollRectExtended.OnSmoothlyScroll(data);
		m_DelayedSelectorMoveDisposable?.Dispose();
	}

	public void SelectPrevPhase()
	{
		base.ViewModel.SelectPrevValidEntity();
	}

	public void SelectNextPhase()
	{
		base.ViewModel.SelectNextValidEntity();
	}

	public void SelectLastValidPhase()
	{
		base.ViewModel.TrySelectLastValidEntity();
	}

	public void SelectFirstValidPhase()
	{
		base.ViewModel.TrySelectFirstValidEntity();
	}
}
