using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI;
using Kingmaker.UI.Common.DebugInformation;
using ObservableCollections;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DetectiveJournalClueView : View<DetectiveJournalClueVM>, IHasBlueprintInfo, ILineTarget, INewIconTarget
{
	[Header("Elements")]
	[SerializeField]
	private CanvasGroup m_CanvasGroup;

	[SerializeField]
	private TMP_Text m_NewAddendumsCount;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	protected OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	protected OwlcatMultiSelectable m_AnswerStateSelectable;

	[SerializeField]
	protected OwlcatMultiSelectable m_SelectedAnswerStateSelectable;

	[SerializeField]
	protected OwlcatMultiSelectable m_NotesStateSelectable;

	[SerializeField]
	private ClueNewData m_NewIconRectTransforms;

	[Header("Values")]
	[SerializeField]
	private Sprite m_DefaultIcon;

	[Header("ConstructConclusions")]
	[SerializeField]
	private WidgetList m_ConclusionsList;

	[SerializeField]
	private ConstructConclusionView m_ConstructConclusionPrefab;

	[SerializeField]
	private CluePointerHandler m_CluePointerHandler;

	[Header("Views")]
	[SerializeField]
	private ClueNameView m_ClueName;

	[Header("Debug")]
	[SerializeField]
	private GameObject m_AnswerLug;

	private RectTransform m_CurrentRt;

	private CaseViewsContext m_CaseContext;

	private readonly ReactiveProperty<RectTransform> m_NewIcon = new ReactiveProperty<RectTransform>();

	[Header("Dots/Lines")]
	[SerializeField]
	private LineView m_LinePrefab;

	[Header("Debug Values")]
	[SerializeField]
	private bool m_UpdateDotsOnEndDrag;

	[SerializeField]
	private float m_ShowClueTime = 0.25f;

	private readonly Dictionary<DetectiveJournalClueView, LineView> m_ClueToLine = new Dictionary<DetectiveJournalClueView, LineView>();

	private LineView m_LineToCard;

	private LineView m_AnimateLine;

	public BlueprintScriptableObject Blueprint => base.ViewModel.Clue;

	[field: SerializeField]
	public RectTransform RectTransform { get; private set; }

	[field: SerializeField]
	public ClueDotsPositions DotsPositions { get; private set; }

	[field: SerializeField]
	public List<LineDirectionData> Directions { get; private set; }

	private JournalPositions<BlueprintClue> Positions => Game.Instance.Player.UISettings.DetectiveSystemData.CluesPositions;

	public RectTransform Parent => RectTransform;

	public ReadOnlyReactiveProperty<RectTransform> NewIcon => m_NewIcon;

	public bool IsUnknownCase => base.ViewModel.Clue.ParentCase.Blueprint.IsUnknown();

	[field: SerializeField]
	public RectTransform ConnectionCenter { get; private set; }

	public void Initialize(CaseViewsContext viewsContext)
	{
		m_CaseContext = viewsContext;
		ObjectExtensions.Or(m_CluePointerHandler, null)?.SetCaseContext(this, viewsContext.CluesContainer);
		DotsPositions.Initialize(this);
	}

	protected override void OnBind()
	{
		SetVisible(state: false);
		m_ClueName.Bind(base.ViewModel.Clue);
		m_Icon.sprite = base.ViewModel.UIData.Icon ?? m_DefaultIcon;
		base.ViewModel.CurrentState.Subscribe(delegate(ClueState value)
		{
			m_StateSelectable.SetActiveLayer(value.ToString());
			UpdateNewIcon();
		}).AddTo(this);
		base.ViewModel.IsAnswerClue.Subscribe(delegate(bool value)
		{
			if (UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ChangeAppearance))
			{
				BlueprintCaseAnswer blueprintCaseAnswer = Game.Instance.DetectiveSystem.GetCaseAnswer(base.ViewModel.Clue.ParentCase)?.Answer;
				bool flag = UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ShowNotSelectedAnswersAsCommon);
				bool flag2 = !base.ViewModel.Clue.ParentCase.Blueprint.IsOpen() || blueprintCaseAnswer?.RelatedItem == base.ViewModel.Clue || !flag;
				bool flag3 = UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ShowHighlightOnSelectedAnswer);
				if (blueprintCaseAnswer?.RelatedItem == base.ViewModel.Clue && flag3)
				{
					m_AnswerStateSelectable.SetActiveLayer(0);
					m_SelectedAnswerStateSelectable.SetActiveLayer(1);
				}
				else
				{
					m_AnswerStateSelectable.SetActiveLayer((value && flag2) ? 1 : 0);
					m_SelectedAnswerStateSelectable.SetActiveLayer(0);
				}
			}
			ObjectExtensions.Or(m_AnswerLug, null)?.SetActive(value && UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ShowLug));
			DotsPositions.UpdateState();
		}).AddTo(this);
		base.ViewModel.NewAddendumsCount.Subscribe(delegate(int value)
		{
			m_NewAddendumsCount.text = $"+{value}";
		}).AddTo(this);
		base.ViewModel.HasNotes.Subscribe(delegate(bool value)
		{
			m_NotesStateSelectable.SetActiveLayer(value ? 1 : 0);
		}).AddTo(this);
		m_CaseContext.Clues.ObserveCountChanged().Skip(1).Subscribe(delegate
		{
			DrawLines();
		})
			.AddTo(this);
		DrawLines();
		SetupPointerHandler();
		if (!IsUnknownCase)
		{
			SetNonEmptyCaseData();
		}
		EventBus.Subscribe(this).AddTo(this);
	}

	protected override void OnUnbind()
	{
		DestroyLines();
	}

	private void SetNonEmptyCaseData()
	{
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.ConclusionsUpdated, delegate
		{
			DrawConclusions();
		}).AddTo(this);
		DrawConclusions();
	}

	private void DrawConclusions()
	{
		m_ConclusionsList.Clear();
		m_ConclusionsList.DrawEntries(base.ViewModel.ConclusionsVM, m_ConstructConclusionPrefab).AddTo(this);
		UpdateNewIcon();
		DotsPositions.UpdateState();
	}

	private void SetupPointerHandler()
	{
		if (!(m_CluePointerHandler == null))
		{
			m_CluePointerHandler.HandleOnPointerEnter = AnimateLineToCard;
			m_CluePointerHandler.HandleOnPointerExit = ClearAnimation;
			m_CluePointerHandler.HandleOnPointerUp = delegate(Vector2 position)
			{
				Positions.UpdatePositionFor(base.ViewModel.Clue, position);
			};
			m_CluePointerHandler.HandleOnPointerClick = delegate
			{
				base.ViewModel.ClickOnClue();
			};
		}
	}

	private void DestroyLines()
	{
		foreach (KeyValuePair<DetectiveJournalClueView, LineView> item in m_ClueToLine)
		{
			item.Deconstruct(out var _, out var value);
			value.Destroy();
		}
		m_ClueToLine.Clear();
		m_LineToCard?.Destroy();
		m_LineToCard = null;
	}

	public void SetVisible(bool state)
	{
		m_CanvasGroup.alpha = (state ? 1f : 0f);
	}

	public void UpdateNewIcon()
	{
		RectTransform rectTransform = m_NewIconRectTransforms.GetNewIcon(base.ViewModel.CurrentState.CurrentValue);
		if (rectTransform == null)
		{
			rectTransform = m_ConclusionsList.Entries.Select((IBindable e) => e as ConstructConclusionView).FirstOrDefault((ConstructConclusionView c) => c?.HasNewConclusion ?? false)?.RectTransform;
		}
		m_NewIcon.Value = rectTransform;
	}

	private void DrawLines()
	{
		DestroyLines();
		foreach (BlueprintClue connectedClue in base.ViewModel.ConnectedClues)
		{
			DetectiveJournalClueView detectiveJournalClueView = m_CaseContext.Clues.FirstOrDefault((DetectiveJournalClueView v) => v.ViewModel.Clue == connectedClue);
			if (!(detectiveJournalClueView == null))
			{
				CreateLineTo(detectiveJournalClueView);
			}
		}
		bool flag = UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ShowAnswersLines) && base.ViewModel.IsAnswerClue.CurrentValue;
		BlueprintCaseAnswer blueprintCaseAnswer = Game.Instance.DetectiveSystem.GetCaseAnswer(base.ViewModel.Clue.ParentCase)?.Answer;
		bool flag2 = !base.ViewModel.Clue.ParentCase.Blueprint.IsClosed() || blueprintCaseAnswer?.RelatedItem == base.ViewModel.Clue;
		flag = flag && flag2;
		if (base.ViewModel.Clue.IsDirectlyConnectedToCase && !flag)
		{
			CreateLineTo(m_CaseContext.CaseCardScreenBaseView);
		}
		if (flag)
		{
			CreateAnswerLine();
		}
	}

	private void CreateLineTo(DetectiveJournalClueView viewTo)
	{
		if (!base.ViewModel.Clue.ParentCase.Blueprint.IsUnknown() && !m_ClueToLine.ContainsKey(viewTo))
		{
			(RectTransform, RectTransform) fromToDots = (ConnectionCenter, viewTo.ConnectionCenter);
			LineView lineView = Object.Instantiate(m_LinePrefab, m_CaseContext.LinesContainer, worldPositionStays: false);
			lineView.Initialize(new ClueLineData(fromToDots));
			m_ClueToLine.Add(viewTo, lineView);
		}
	}

	private void CreateLineTo(CaseCardScreenBaseView caseCard)
	{
		if (!base.ViewModel.Clue.ParentCase.Blueprint.IsUnknown())
		{
			(RectTransform, RectTransform) fromToDots = (ConnectionCenter, caseCard.ConnectionCenter);
			m_LineToCard = Object.Instantiate(m_LinePrefab, m_CaseContext.LinesContainer, worldPositionStays: false);
			m_LineToCard.Initialize(new ClueLineData(fromToDots));
		}
	}

	private void CreateAnswerLine()
	{
		if (!base.ViewModel.Clue.ParentCase.Blueprint.IsUnknown())
		{
			(RectTransform, RectTransform) fromToDots = (ConnectionCenter, m_CaseContext.CaseCardScreenBaseView.ConnectionCenter);
			m_LineToCard = Object.Instantiate(m_LinePrefab, m_CaseContext.LinesContainer, worldPositionStays: false);
			LineType lineType = ((!base.ViewModel.Clue.ParentCase.Blueprint.IsClosed()) ? LineType.Answer : LineType.AnswerSelected);
			m_LineToCard.Initialize(new ClueLineData(fromToDots), lineType);
		}
	}

	private void AnimateLineToCard()
	{
		if (base.ViewModel.IsAnswerClue.CurrentValue && base.ViewModel.Clue.ParentCase.Blueprint.IsOpen() && UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.AnimateAnswersLines))
		{
			(RectTransform, RectTransform) fromToDots = (ConnectionCenter, m_CaseContext.CaseCardScreenBaseView.ConnectionCenter);
			m_AnimateLine = Object.Instantiate(m_LinePrefab, m_CaseContext.LinesContainer, worldPositionStays: false);
			m_AnimateLine.Initialize(new ClueLineData(fromToDots), LineType.Answer);
			m_AnimateLine.Animate();
		}
	}

	private void ClearAnimation()
	{
		if (base.ViewModel.IsAnswerClue.CurrentValue && base.ViewModel.Clue.ParentCase.Blueprint.IsOpen())
		{
			m_AnimateLine?.Destroy();
		}
	}
}
