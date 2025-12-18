using System;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.UI.Common;
using Kingmaker.UI.Sound;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenRoadmapMenuView : View<SelectionGroupRadioVM<CharGenPhaseBaseVM>>
{
	private const float SELECTOR_LEVEL_OFFSET = 12f;

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

	[Space]
	[SerializeField]
	protected CharGenPregenPhaseRoadmapView m_PregenPhaseRoadmapView;

	[SerializeField]
	protected CharGenAppearancePhaseRoadmapView m_AppearancePhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_SoulMarkPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_HomeworldPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_ImperialHomeworldChildPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_ForgeHomeworldChildPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_SanctionedPsykerChildPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_OccupationPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_NavigatorPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_MomentOfTriumphPhaseRoadmapView;

	[SerializeField]
	protected CharGenBackgroundBasePhaseRoadmapView m_DarkestHourPhaseRoadmapView;

	[SerializeField]
	protected CharGenCareerPhaseRoadmapView m_CareerPhaseRoadmapView;

	[SerializeField]
	private CharGenAttributesPhaseRoadmapView m_AttributesPhaseRoadmapView;

	[SerializeField]
	private CharGenShipPhaseRoadmapView m_ShipPhaseRoadmapView;

	[SerializeField]
	private CharGenSummaryPhaseRoadmapView m_SummaryPhaseRoadmapView;

	[SerializeField]
	private RectTransform m_Selector;

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

	public void Initialize()
	{
	}

	protected override void OnBind()
	{
		ClearRoadmapPhaseViews();
		foreach (CharGenPhaseBaseVM item in base.ViewModel.EntitiesCollection)
		{
			CreateRoadmapPhaseView(item);
		}
		base.ViewModel.EntitiesCollection.ObserveAdd().Subscribe(CreateRoadmapPhaseView).AddTo(this);
		base.ViewModel.EntitiesCollection.ObserveRemove().Subscribe(RemoveRoadmapPhaseView).AddTo(this);
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
		foreach (CharGenCommonPhaseRoadmapView item in m_PhaseViews.Where((CharGenCommonPhaseRoadmapView v) => v.ViewModel == removeEvent.Value || v.ViewModel == null))
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
		m_DelayedSelectorMoveDisposable = ObservableSubscribeExtensions.Subscribe(Observable.IntervalFrame(2), delegate
		{
			m_ScrollRectExtended.EnsureVisibleHorizontal(m_SelectedView.ViewRectTransform, 160f);
			float endValue = m_SelectedView.ViewRectTransform.localPosition.x + ((selectedEntity.Rank > 0) ? 12f : 0f);
			m_Selector.DOLocalMoveX(endValue, m_AnimationDuration).OnStart(delegate
			{
				if (m_PrevEntity != selectedEntity)
				{
					UISounds.Instance.Sounds.Selector.SelectorStart.Play();
					UISounds.Instance.Sounds.Selector.SelectorLoopStart.Play();
					m_SelectorSoundPlaying = true;
					m_PrevEntity = selectedEntity;
				}
			}).OnComplete(ShutUpSelector)
				.OnKill(ShutUpSelector)
				.SetUpdate(isIndependentUpdate: true);
		}).AddTo(this);
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
			UISounds.Instance.Sounds.Selector.SelectorStop.Play();
		}
		UISounds.Instance.Play(UISounds.Instance.Sounds.Selector.SelectorLoopStop, isButton: false, playAnyway: true);
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
		charGenCommonPhaseRoadmapView.transform.SetSiblingIndex(m_PhaseViews.Count);
		m_PhaseViews.Add(charGenCommonPhaseRoadmapView);
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
	}

	private void SmoothScrollBy(float delta)
	{
		PointerEventData data = new PointerEventData(EventSystem.current)
		{
			scrollDelta = new Vector2(delta, 0f)
		};
		m_ScrollRectExtended.OnSmoothlyScroll(data);
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
