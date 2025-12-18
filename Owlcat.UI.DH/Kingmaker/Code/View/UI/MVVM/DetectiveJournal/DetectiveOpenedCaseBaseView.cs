using System.Collections.Generic;
using System.Linq;
using Code.View.UI.MVVM.DetectiveJournal.CaseCard;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.Events;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveOpenedCaseBaseView : View<DetectiveOpenedCaseVM>, IInitializable, IMoveToCaseItemHandler, ISubscriber
{
	[Header("CluesPlacement")]
	[SerializeField]
	private Vector2Int m_GridSize = new Vector2Int(11, 5);

	[SerializeField]
	private float m_HeightWeight = 1.25f;

	[SerializeField]
	private float m_AreaWeight = 3f;

	private readonly Vector2 m_MockClueSize = new Vector2(320f, 320f);

	[Header("Views")]
	[SerializeField]
	private OpenedCaseScreenBaseView m_OpenedCaseScreenBaseView;

	[SerializeField]
	private AnswersListView m_AnswersListView;

	[SerializeField]
	private DetectiveReportBaseView m_ReportView;

	[SerializeField]
	private AnswerTierChangeView m_AnswerTierChangeView;

	[SerializeField]
	private ClueFullInfoView m_ClueFullInfoView;

	[FormerlySerializedAs("m_NewConclusionSelectionWindowView")]
	[SerializeField]
	private ConclusionSelectionWindowView ConclusionSelectionWindowView;

	[SerializeField]
	private NewCluesMarkersView m_NewCluesMarkersView;

	[SerializeField]
	private CaseCardScreenBaseView m_CaseCardScreenBaseView;

	[SerializeField]
	private TypeToPrefabSelector<DetectiveJournalClueView> m_Prefabs;

	[SerializeField]
	private DeductionOnScreenView m_DeductionPrefab;

	[Header("Elements")]
	[SerializeField]
	private GameObject m_PaperElementsParent;

	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private RectTransform m_MoveScaleTransform;

	[SerializeField]
	private RectTransform m_LinesContainer;

	[SerializeField]
	private RectTransform m_CluesContainer;

	[SerializeField]
	private CanvasGroup m_CluesGroup;

	private readonly ObservableList<DetectiveJournalClueView> m_Views = new ObservableList<DetectiveJournalClueView>();

	private readonly ObservableList<DeductionOnScreenView> m_Conclusions = new ObservableList<DeductionOnScreenView>();

	private readonly ObservableList<INewIconTarget> m_NewCluesMarkers = new ObservableList<INewIconTarget>();

	private CaseViewsContext m_CaseViewsContext;

	private Tweener m_MoveTweener;

	private Vector2 m_Size;

	private bool m_ShouldUpdateConclusions;

	private float m_StartedSizeX = 4000f;

	private Vector2 GetSuitablePositionFor(DetectiveJournalClueView view)
	{
		List<Vector2> placementPoints = CluesPositionsUtils.GetPlacementPoints(new Vector2(Mathf.Max(6000f, m_StartedSizeX), Mathf.Max(4500f, m_StartedSizeX * 0.85f)), m_GridSize);
		Vector2 randomRange = new Vector2(100f, 50f);
		placementPoints = placementPoints.Select((Vector2 pt) => pt + new Vector2(Random.Range(0f - randomRange.x, randomRange.x), Random.Range(0f - randomRange.y, randomRange.y))).ToList();
		List<BlueprintClue> connected = UIUtilityDetective.GetAllConnectedClues(view.ViewModel.Clue);
		Vector2[] connectedPositions = (from v in m_Views
			where connected.Contains(v.ViewModel.Clue)
			select v.RectTransform.anchoredPosition).ToArray();
		if (!connectedPositions.Any())
		{
			connectedPositions = new Vector2[1] { Vector2.zero };
		}
		return placementPoints.OrderBy((Vector2 p) => CluePlacementHeuristic(p, connectedPositions)).FirstOrDefault();
	}

	private float CluePlacementHeuristic(Vector2 position, params Vector2[] connectedClues)
	{
		float num = 0f;
		Rect r = new Rect(position - m_MockClueSize * 0.5f, m_MockClueSize);
		foreach (DetectiveJournalClueView view in m_Views)
		{
			RectTransform rectTransform = view.RectTransform;
			Rect r2 = new Rect(rectTransform.anchoredPosition - rectTransform.rect.size * 0.5f, rectTransform.rect.size);
			if (UIUtilityRect.Intersects(r, r2, out var area))
			{
				num += area.width * area.height;
			}
		}
		foreach (DeductionOnScreenView conclusion in m_Conclusions)
		{
			RectTransform rectTransform2 = conclusion.RectTransform;
			Rect r3 = new Rect(rectTransform2.anchoredPosition - rectTransform2.rect.size * 0.5f, rectTransform2.rect.size);
			if (UIUtilityRect.Intersects(r, r3, out var area2))
			{
				num += area2.width * area2.height;
			}
		}
		return connectedClues.Sum(delegate(Vector2 clue)
		{
			Vector2 vector = clue - position;
			vector.y *= m_HeightWeight;
			return vector.magnitude;
		}) / (float)connectedClues.Length + num * m_AreaWeight;
	}

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
		m_PaperElementsParent.SetActive(value: false);
	}

	protected override void OnBind()
	{
		m_CaseViewsContext = new CaseViewsContext(m_CaseCardScreenBaseView, m_Views, m_Conclusions, m_LinesContainer, m_CluesContainer);
		m_CaseCardScreenBaseView.Initialize(m_CaseViewsContext);
		m_CaseCardScreenBaseView.Bind(base.ViewModel.CaseCardVM);
		m_AnswerTierChangeView.Bind(base.ViewModel.AnswerTierChangeVM);
		m_StartedSizeX = Mathf.Max(2000f + 500f * (float)base.ViewModel.Clues.Count, 4000f);
		m_MoveScaleTransform.sizeDelta = new Vector2(m_StartedSizeX, m_StartedSizeX * 0.85f);
		base.ViewModel.ReportVM.Subscribe(HandleReport).AddTo(this);
		base.ViewModel.ClueInfoVM.Subscribe(delegate(ClueFullInfoVM value)
		{
			m_ClueFullInfoView.Bind(value);
			m_OpenedCaseScreenBaseView.SetVisibleState(value == null);
		}).AddTo(this);
		base.ViewModel.NewDeductionSelectionVM.Subscribe(delegate(ConclusionSelectionWindowVM value)
		{
			if (!UIConfig.Instance.DetectiveConfig.UseDefaultConclusionWindow)
			{
				ConclusionSelectionWindowView.Bind(value);
				m_OpenedCaseScreenBaseView.SetVisibleState(value == null);
			}
		}).AddTo(this);
		base.ViewModel.Clues.ObserveAdd().Subscribe(delegate(CollectionAddEvent<DetectiveJournalClueVM> value)
		{
			DrawClue(value.Value);
		}).AddTo(this);
		DrawClues();
		DrawDeductions();
		base.ViewModel.Conclusions.ObserveCountChanged().Subscribe(delegate
		{
			m_ShouldUpdateConclusions = true;
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.PreLateUpdate), delegate
		{
			UpdateConclusions();
		}).AddTo(this);
		m_OpenedCaseScreenBaseView.Bind(base.ViewModel.OpenedCaseScreenVM);
		m_NewCluesMarkers.Clear();
		m_NewCluesMarkers.AddRange(m_Views);
		m_NewCluesMarkers.AddRange(m_Conclusions);
		m_NewCluesMarkers.Add(m_CaseCardScreenBaseView);
		m_NewCluesMarkersView.Bind(m_NewCluesMarkers);
		m_Conclusions.ObserveAdd().Subscribe(delegate(CollectionAddEvent<DeductionOnScreenView> view)
		{
			m_NewCluesMarkers.Add(view.Value);
		}).AddTo(this);
		m_Conclusions.ObserveRemove().Subscribe(delegate(CollectionRemoveEvent<DeductionOnScreenView> view)
		{
			m_NewCluesMarkers.Remove(view.Value);
		}).AddTo(this);
		base.gameObject.SetActive(value: true);
		m_PaperElementsParent.SetActive(value: true);
		bool flag = UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ShowAnswersListAtCard);
		if (flag)
		{
			m_AnswersListView.Bind(base.ViewModel.AnswersListVM);
		}
		m_AnswersListView.gameObject.SetActive(flag && base.ViewModel.BlueprintCase.IsOpen());
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
		m_PaperElementsParent.SetActive(value: false);
		m_Views.ForEach(WidgetFactory.DisposeWidget);
		m_Views.Clear();
		m_Conclusions.ForEach(WidgetFactory.DisposeWidget);
		m_Conclusions.Clear();
		m_NewCluesMarkersView.Unbind();
	}

	private void UpdateConclusions()
	{
		if (m_ShouldUpdateConclusions)
		{
			m_CaseCardScreenBaseView.UpdateNewIcon();
			DrawDeductions();
			m_ShouldUpdateConclusions = false;
		}
	}

	private void HandleReport(DetectiveReportVM reportVM)
	{
		m_ReportView.Bind(reportVM);
		SetVisible(reportVM == null);
		m_CaseCardScreenBaseView.UpdateNewIcon();
	}

	private void SetVisible(bool state)
	{
		if (state)
		{
			m_FadeAnimator.AppearAnimation();
			m_PaperElementsParent.SetActive(value: true);
		}
		else
		{
			m_FadeAnimator.DisappearAnimation();
			m_PaperElementsParent.SetActive(value: false);
		}
	}

	private void DrawClues()
	{
		foreach (DetectiveJournalClueVM clue in base.ViewModel.Clues)
		{
			DrawClue(clue);
		}
	}

	private void DrawClue(DetectiveJournalClueVM clueVM)
	{
		JournalPositions<BlueprintClue> positions = Game.Instance.Player.UISettings.DetectiveSystemData.CluesPositions;
		bool shouldUpdatePosition = false;
		DetectiveJournalClueView entity = m_Prefabs.GetEntity(clueVM.UIData.UIType);
		DetectiveJournalClueView view = WidgetFactory.GetWidget(entity, activate: true, strictMatching: true);
		view.transform.SetParent(m_CluesContainer, worldPositionStays: false);
		view.Initialize(m_CaseViewsContext);
		view.Bind(clueVM);
		if (positions.TryGetPositionsFor(clueVM.Clue, out var position))
		{
			view.RectTransform.anchoredPosition = position;
		}
		else
		{
			view.RectTransform.anchoredPosition = new Vector2(-10000f, -10000f);
			shouldUpdatePosition = true;
		}
		view.SetVisible(state: true);
		m_Views.Add(view);
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			if (shouldUpdatePosition)
			{
				Vector2 suitablePositionFor = GetSuitablePositionFor(view);
				positions.UpdatePositionFor(clueVM.Clue, suitablePositionFor);
				view.RectTransform.anchoredPosition = suitablePositionFor;
			}
		}).AddTo(this);
	}

	private void DrawDeductions()
	{
		foreach (DeductionOnScreenView conclusion in m_Conclusions)
		{
			WidgetFactory.DisposeWidget(conclusion);
		}
		m_Conclusions.Clear();
		foreach (DeductionOnScreenVM item in base.ViewModel.Conclusions.OrderBy(delegate(DeductionOnScreenVM vm)
		{
			UIUtilityDetective.GetNestingCount(vm.Conclusion, out var nestingCount);
			return nestingCount;
		}).ToList())
		{
			DeductionOnScreenView widget = WidgetFactory.GetWidget(m_DeductionPrefab, activate: true, strictMatching: true);
			widget.transform.SetParent(m_CluesContainer, worldPositionStays: false);
			widget.Initialize(m_CaseViewsContext);
			widget.Bind(item);
			m_Conclusions.Add(widget);
		}
	}

	public void HandleMoveToCaseItem(BlueprintCaseItem caseItem)
	{
		Transform transform = m_Views.FirstOrDefault((DetectiveJournalClueView v) => v.ViewModel.Clue == caseItem)?.transform ?? m_Conclusions.FirstOrDefault((DeductionOnScreenView v) => v.ViewModel.Conclusion == caseItem)?.transform;
		if (transform != null)
		{
			HandleMoveToItemPosition(transform.position);
		}
	}

	public void HandleMoveToItemPosition(Vector3 position)
	{
		m_OpenedCaseScreenBaseView.HandleMoveToPosition(position);
	}
}
