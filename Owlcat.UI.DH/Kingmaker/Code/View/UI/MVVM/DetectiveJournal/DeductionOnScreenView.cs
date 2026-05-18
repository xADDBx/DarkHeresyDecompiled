using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI;
using ObservableCollections;
using Owlcat.Fmw.Blueprints;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DeductionOnScreenView : View<DeductionOnScreenVM>, ILineTarget, INewIconTarget
{
	[Header("Elements")]
	[SerializeField]
	private DeductionPointerHandler m_PointerHandler;

	[SerializeField]
	private RectTransform m_NewIconRectTransform;

	[SerializeField]
	private GameObject m_IsReportEntity;

	[SerializeField]
	private TMP_Text m_DeductionLabel;

	[SerializeField]
	private OwlcatMultiButton m_ToDeductionButton;

	[SerializeField]
	private ConstructConclusionView m_MakeDeductionButton;

	[SerializeField]
	private OwlcatMultiSelectable m_RefutedStateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_NewStateSelectable;

	[Header("Lines")]
	[SerializeField]
	private NewStraightLine m_NewStraightLinePrefab;

	[SerializeField]
	private Gradient m_LineColor;

	[SerializeField]
	private Gradient m_RefutedLineColor;

	[Header("Debug")]
	[SerializeField]
	private bool m_UpdateDirections = true;

	private RectTransform m_CluesContainer;

	private RectTransform m_LinesContainer;

	private DetectiveOpenedCaseBaseView m_OpenedCaseView;

	private ObservableList<DetectiveJournalClueView> m_Views = new ObservableList<DetectiveJournalClueView>();

	private ObservableList<DeductionOnScreenView> m_Conclusions;

	private readonly ReactiveProperty<RectTransform> m_NewIcon = new ReactiveProperty<RectTransform>();

	private ILineTarget m_CaseItemFrom;

	private ILineTarget m_CaseItemTo;

	private readonly List<NewStraightLine> m_NewStraightLines = new List<NewStraightLine>();

	[field: SerializeField]
	public RectTransform RectTransform { get; private set; }

	[field: SerializeField]
	public ClueDotsPositions DotsPositions { get; private set; }

	[field: SerializeField]
	public List<LineDirectionData> Directions { get; private set; }

	public RectTransform Parent => RectTransform;

	public ReadOnlyReactiveProperty<RectTransform> NewIcon => m_NewIcon;

	private JournalPositions<BlueprintConclusion> Positions => Game.Instance.Player.UISettings.DetectiveSystemData.ConclusionPositions;

	public void Initialize(CaseViewsContext viewsContext)
	{
		m_Views = viewsContext.Clues;
		m_Conclusions = viewsContext.Conclusions;
		m_LinesContainer = viewsContext.LinesContainer;
		m_CluesContainer = viewsContext.CluesContainer;
		m_OpenedCaseView = viewsContext.OpenedCaseView;
	}

	protected override void OnBind()
	{
		m_DeductionLabel.text = base.ViewModel.Conclusion.Description;
		m_CaseItemFrom = GetTarget(base.ViewModel.SelectedSource.Item1);
		m_CaseItemTo = GetTarget(base.ViewModel.SelectedSource.Item2);
		SetPosition();
		m_PointerHandler.Initialize(this, m_CaseItemFrom, m_CaseItemTo, base.transform.parent.GetComponent<RectTransform>());
		m_PointerHandler.HandleOnPointerClick = delegate
		{
			base.ViewModel.OnConclusionClick();
		};
		base.ViewModel.ConstructConclusionVM.Subscribe(delegate(ConstructConclusionVM value)
		{
			UpdateNewIcon();
			m_MakeDeductionButton.Bind(value);
		}).AddTo(this);
		base.ViewModel.CanMakeDeduction.Subscribe(delegate(bool value)
		{
			m_MakeDeductionButton.gameObject.SetActive(value);
		}).AddTo(this);
		base.ViewModel.RefutedConclusion.Subscribe(delegate(bool value)
		{
			m_RefutedStateSelectable.SetActiveLayer(value ? 1 : 0);
			UpdateLinesColor();
		}).AddTo(this);
		base.ViewModel.IsNewConclusion.Subscribe(delegate(bool value)
		{
			m_NewStateSelectable.SetActiveLayer(value ? 1 : 0);
			UpdateNewIcon();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.Refresh, delegate
		{
			DrawPagination();
			UpdateNewIcon();
		}).AddTo(this);
		DrawPagination();
		if (Positions.TryGetPositionsFor(base.ViewModel.Conclusion, out var position))
		{
			m_PointerHandler.SetupPosition(position);
		}
		SetupLines();
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(UnityFrameProvider.Update), delegate
		{
			if (m_CaseItemFrom != null && m_CaseItemTo != null)
			{
				UpdateDirections();
				UpdateDots();
			}
		}).AddTo(this);
		UpdateDirections();
		UpdateDots();
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		base.gameObject.SetActive(value: false);
		m_NewStraightLines.ForEach(delegate(NewStraightLine l)
		{
			UnityEngine.Object.Destroy(l.gameObject);
		});
		m_NewStraightLines.Clear();
		m_CaseItemFrom.DotsPositions.DestroyDotsFor(base.ViewModel.Conclusion);
		m_CaseItemTo.DotsPositions.DestroyDotsFor(base.ViewModel.Conclusion);
		ForgetSharedPositionIfSlotEmpty();
	}

	private void ForgetSharedPositionIfSlotEmpty()
	{
		BlueprintCaseItem blueprintCaseItem = base.ViewModel?.SelectedSource?.Item1;
		if (blueprintCaseItem == null)
		{
			return;
		}
		List<BlueprintConclusion> list = EnumerateSlotSiblings(blueprintCaseItem).ToList();
		DetectiveSystem detective = Game.Instance.DetectiveSystem;
		if (list.Any((BlueprintConclusion c) => detective.HasConclusionExcludingHidden(c)))
		{
			return;
		}
		foreach (BlueprintConclusion item in list)
		{
			Positions.TryRemovePositionFor(item);
		}
	}

	private void DrawPagination()
	{
	}

	private void SetPosition()
	{
		foreach (BlueprintConclusion item in (from c in EnumerateSlotSiblings(base.ViewModel.SelectedSource?.Item1)
			where c != base.ViewModel.Conclusion
			select c).Concat(new BlueprintConclusion[1] { base.ViewModel.Conclusion }).ToList())
		{
			if (Positions.TryGetPositionsFor(item, out var position))
			{
				RectTransform.anchoredPosition = position;
				SaveSharedPosition(position);
				return;
			}
		}
		RectTransform.anchoredPosition = new Vector2(-10000f, -10000f);
		ObservableSubscribeExtensions.Subscribe(Observable.NextFrame(), delegate
		{
			PlaceWithHeuristic();
		}).AddTo(this);
	}

	private void PlaceWithHeuristic()
	{
		if (!(m_OpenedCaseView == null) && !(RectTransform == null) && !Positions.HasPositionFor(base.ViewModel.Conclusion))
		{
			Vector2 fromPos = ((m_CaseItemFrom?.RectTransform != null) ? m_CaseItemFrom.RectTransform.anchoredPosition : Vector2.zero);
			Vector2 toPos = ((m_CaseItemTo?.RectTransform != null) ? m_CaseItemTo.RectTransform.anchoredPosition : Vector2.zero);
			Vector2 suitablePositionForConclusion = m_OpenedCaseView.GetSuitablePositionForConclusion(this, fromPos, toPos);
			RectTransform.anchoredPosition = suitablePositionForConclusion;
			SaveSharedPosition(suitablePositionForConclusion);
			m_PointerHandler.SetupPosition(suitablePositionForConclusion);
		}
	}

	public void SaveSharedPosition(Vector2 position)
	{
		if (base.ViewModel?.Conclusion != null)
		{
			Positions.UpdatePositionFor(base.ViewModel.Conclusion, position);
		}
		BlueprintCaseItem blueprintCaseItem = base.ViewModel?.SelectedSource?.Item1;
		if (blueprintCaseItem == null)
		{
			return;
		}
		foreach (BlueprintConclusion item in EnumerateSlotSiblings(blueprintCaseItem))
		{
			if (item != base.ViewModel.Conclusion)
			{
				Positions.UpdatePositionFor(item, position);
			}
		}
	}

	private static IEnumerable<BlueprintConclusion> EnumerateSlotSiblings(BlueprintCaseItem item1)
	{
		if (item1?.PossibleConclusions == null)
		{
			yield break;
		}
		BpRef<BlueprintConclusion>[] possibleConclusions = item1.PossibleConclusions;
		foreach (BlueprintConclusion blueprintConclusion in possibleConclusions)
		{
			if (blueprintConclusion != null && blueprintConclusion.Sources != null && blueprintConclusion.Sources.Any((BlueprintConclusion.Source s) => s.Item1 == item1))
			{
				yield return blueprintConclusion;
			}
		}
	}

	private ILineTarget GetTarget(BlueprintCaseItem caseItem)
	{
		BlueprintClue blueprintClue = caseItem as BlueprintClue;
		if (blueprintClue == null)
		{
			BlueprintClueAddendum blueprintClueAddendum = caseItem as BlueprintClueAddendum;
			if (blueprintClueAddendum == null)
			{
				BlueprintConclusion blueprintConclusion = caseItem as BlueprintConclusion;
				if (blueprintConclusion != null)
				{
					return m_Conclusions.FirstOrDefault((DeductionOnScreenView v) => v.ViewModel.Conclusion == blueprintConclusion);
				}
				throw new ArgumentOutOfRangeException("caseItem");
			}
			return m_Views.FirstOrDefault((DetectiveJournalClueView v) => v.ViewModel.Clue == blueprintClueAddendum.ParentClue.Blueprint);
		}
		return m_Views.FirstOrDefault((DetectiveJournalClueView v) => v.ViewModel.Clue == blueprintClue || v.ViewModel.Clue.HasOverride(blueprintClue));
	}

	private void SetupLines()
	{
		float time = UnityEngine.Random.Range(0f, 1f);
		Color value = (base.ViewModel.RefutedConclusion.CurrentValue ? m_RefutedLineColor : m_LineColor).Evaluate(time);
		List<LineDirectionData> list = new List<LineDirectionData>
		{
			new LineDirectionData(m_CaseItemFrom.RectTransform, LineDirection.None, 100f),
			new LineDirectionData(Directions.ElementAt(0).Dot, LineDirection.Left, -100f)
		};
		List<LineDirectionData> list2 = new List<LineDirectionData>
		{
			new LineDirectionData(Directions.ElementAt(1).Dot, LineDirection.Right, 100f),
			new LineDirectionData(m_CaseItemTo.RectTransform, LineDirection.None, -100f)
		};
		NewStraightLine newStraightLine = UnityEngine.Object.Instantiate(m_NewStraightLinePrefab, m_LinesContainer, worldPositionStays: false);
		newStraightLine.Initialize(list[0], list[1], m_CluesContainer, value);
		m_NewStraightLines.Add(newStraightLine);
		NewStraightLine newStraightLine2 = UnityEngine.Object.Instantiate(m_NewStraightLinePrefab, m_LinesContainer, worldPositionStays: false);
		newStraightLine2.Initialize(list2[0], list2[1], m_CluesContainer, value);
		m_NewStraightLines.Add(newStraightLine2);
	}

	public void UpdateLinesColor()
	{
		float time = UnityEngine.Random.Range(0f, 1f);
		Gradient gradient = (base.ViewModel.RefutedConclusion.CurrentValue ? m_RefutedLineColor : m_LineColor);
		Color lineColor = gradient.Evaluate(time);
		m_NewStraightLines.ForEach(delegate(NewStraightLine l)
		{
			l.SetColor(lineColor);
		});
	}

	private void UpdateDirections()
	{
		if (!m_UpdateDirections)
		{
			return;
		}
		if (m_CaseItemFrom is DetectiveJournalClueView detectiveJournalClueView)
		{
			LineDirectionData lineDirectionData = ((m_CaseItemFrom.RectTransform.position.x < m_CaseItemTo.RectTransform.position.x) ? Directions.ElementAt(0) : Directions.ElementAt(1));
			Vector3 fromPosition = lineDirectionData.Dot.position;
			if (detectiveJournalClueView.DotsPositions != null)
			{
				LineDirection closestDirectionTo = detectiveJournalClueView.DotsPositions.GetClosestDirectionTo(m_CluesContainer.InverseTransformPoint(lineDirectionData.Dot.position) + lineDirectionData.Direction.GetDirectionVector() * lineDirectionData.Length, m_CluesContainer);
				if (closestDirectionTo != m_NewStraightLines.ElementAt(0).StartPoint.Direction)
				{
					DotWidget dotByDirection = detectiveJournalClueView.DotsPositions.GetDotByDirection(base.ViewModel.Conclusion, closestDirectionTo);
					m_NewStraightLines.ElementAt(0).SetDirection(0, closestDirectionTo);
					m_NewStraightLines.ElementAt(0).UpdateDot(0, dotByDirection.RectTransform);
				}
			}
			else
			{
				LineDirectionData lineDirectionData2 = m_CaseItemFrom.Directions.OrderBy((LineDirectionData d) => Vector3.Distance(fromPosition, d.Dot.position)).FirstOrDefault();
				if (lineDirectionData2 != null)
				{
					m_NewStraightLines[0].SetDirection(0, lineDirectionData2);
				}
			}
		}
		else
		{
			LineDirection closestDirectionTo2 = m_CaseItemFrom.DotsPositions.GetClosestDirectionTo(m_CluesContainer.InverseTransformPoint(RectTransform.position), m_CluesContainer);
			if (closestDirectionTo2 != m_NewStraightLines.ElementAt(0).StartPoint.Direction)
			{
				DotWidget dotByDirection2 = m_CaseItemFrom.DotsPositions.GetDotByDirection(base.ViewModel.Conclusion, closestDirectionTo2);
				m_NewStraightLines.ElementAt(0).SetDirection(0, closestDirectionTo2);
				m_NewStraightLines.ElementAt(0).UpdateDot(0, dotByDirection2.RectTransform);
			}
		}
		if (m_CaseItemTo is DetectiveJournalClueView detectiveJournalClueView2)
		{
			LineDirectionData lineDirectionData3 = ((m_CaseItemFrom.RectTransform.position.x < m_CaseItemTo.RectTransform.position.x) ? Directions.ElementAt(1) : Directions.ElementAt(0));
			Vector3 fromPosition = lineDirectionData3.Dot.position;
			if (detectiveJournalClueView2.DotsPositions != null)
			{
				LineDirection closestDirectionTo3 = detectiveJournalClueView2.DotsPositions.GetClosestDirectionTo(m_CluesContainer.InverseTransformPoint(lineDirectionData3.Dot.position) + lineDirectionData3.Direction.GetDirectionVector() * lineDirectionData3.Length, m_CluesContainer);
				if (closestDirectionTo3 != m_NewStraightLines.ElementAt(1).EndPoint.Direction)
				{
					DotWidget dotByDirection3 = detectiveJournalClueView2.DotsPositions.GetDotByDirection(base.ViewModel.Conclusion, closestDirectionTo3);
					LineDirectionData lineDirectionData4 = m_NewStraightLines.ElementAt(1).EndPoint;
					lineDirectionData4.Direction = closestDirectionTo3;
					lineDirectionData4.Dot = dotByDirection3.RectTransform;
					if (lineDirectionData4.Length > 0f)
					{
						lineDirectionData4 = lineDirectionData4.Negate();
					}
					m_NewStraightLines.ElementAt(1).SetDirection(1, lineDirectionData4);
				}
			}
			else
			{
				LineDirectionData lineDirectionData5 = m_CaseItemTo.Directions.OrderBy((LineDirectionData d) => Vector3.Distance(fromPosition, d.Dot.position)).FirstOrDefault();
				if (lineDirectionData5 != null)
				{
					m_NewStraightLines[1].SetDirection(1, lineDirectionData5.Negate());
				}
			}
			return;
		}
		LineDirection closestDirectionTo4 = m_CaseItemTo.DotsPositions.GetClosestDirectionTo(m_CluesContainer.InverseTransformPoint(RectTransform.position), m_CluesContainer);
		if (closestDirectionTo4 != m_NewStraightLines.ElementAt(1).EndPoint.Direction)
		{
			DotWidget dotByDirection4 = m_CaseItemTo.DotsPositions.GetDotByDirection(base.ViewModel.Conclusion, closestDirectionTo4);
			LineDirectionData lineDirectionData6 = m_NewStraightLines.ElementAt(1).EndPoint;
			lineDirectionData6.Direction = closestDirectionTo4;
			lineDirectionData6.Dot = dotByDirection4.RectTransform;
			if (lineDirectionData6.Length > 0f)
			{
				lineDirectionData6 = lineDirectionData6.Negate();
			}
			m_NewStraightLines.ElementAt(1).SetDirection(1, lineDirectionData6);
		}
	}

	private void UpdateDots()
	{
		if (m_UpdateDirections)
		{
			if (m_CaseItemFrom.RectTransform.position.x < m_CaseItemTo.RectTransform.position.x)
			{
				Directions.ElementAt(0).Direction = LineDirection.Left;
				Directions.ElementAt(1).Direction = LineDirection.Right;
				m_NewStraightLines[0].SetDirection(1, Directions.ElementAt(0).Negate());
				m_NewStraightLines[1].SetDirection(0, Directions.ElementAt(1));
			}
			else
			{
				Directions.ElementAt(1).Direction = LineDirection.Right;
				Directions.ElementAt(0).Direction = LineDirection.Left;
				m_NewStraightLines[0].SetDirection(1, Directions.ElementAt(1).Negate());
				m_NewStraightLines[1].SetDirection(0, Directions.ElementAt(0));
			}
			if (m_CaseItemFrom is DeductionOnScreenView)
			{
				m_NewStraightLines.ElementAt(0).SetLength(0, 40f);
			}
			if (m_CaseItemTo is DeductionOnScreenView)
			{
				m_NewStraightLines.ElementAt(1).SetLength(1, -40f);
			}
		}
	}

	private void UpdateNewIcon()
	{
		if (base.ViewModel.IsNewConclusion.CurrentValue)
		{
			m_NewIcon.Value = m_NewIconRectTransform;
			return;
		}
		ConstructConclusionVM currentValue = base.ViewModel.ConstructConclusionVM.CurrentValue;
		if (currentValue != null && currentValue.HasNewConclusion.CurrentValue)
		{
			m_NewIcon.Value = m_MakeDeductionButton.RectTransform;
		}
		else
		{
			m_NewIcon.Value = null;
		}
	}
}
