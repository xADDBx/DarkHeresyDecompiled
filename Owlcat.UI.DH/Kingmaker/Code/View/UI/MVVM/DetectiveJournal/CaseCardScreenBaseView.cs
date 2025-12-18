using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Framework.DetectiveSystem;
using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class CaseCardScreenBaseView : CaseCardBaseView, INewIconTarget
{
	[Header("Views")]
	[SerializeField]
	private TypeToPrefabSelector<ExpandableAnswerView> m_AnswerPrefabs;

	[SerializeField]
	private RectTransform m_NewIcon;

	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_Button;

	[SerializeField]
	private RectTransform m_AnswersContainer;

	[Header("Dots/Lines")]
	[SerializeField]
	private LineView m_LinePrefab;

	private List<LineView> m_AnimateLines = new List<LineView>();

	private readonly ReactiveProperty<RectTransform> m_NewIconRp = new ReactiveProperty<RectTransform>();

	private List<ExpandableAnswerView> m_Answers = new List<ExpandableAnswerView>();

	private CaseViewsContext m_ViewsContext;

	[field: Header("Elements")]
	[field: SerializeField]
	public RectTransform ConnectionCenter { get; private set; }

	[field: SerializeField]
	public RectTransform RectTransform { get; private set; }

	public RectTransform Parent => RectTransform;

	public ReadOnlyReactiveProperty<RectTransform> NewIcon => m_NewIconRp;

	public void Initialize(CaseViewsContext viewsContext)
	{
		m_ViewsContext = viewsContext;
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.CurrentState.Subscribe(delegate(CardState s)
		{
			m_StateSelectable.SetActiveLayer(s.ToString());
			m_Button.SetActiveLayer(s.ToString());
		}).AddTo(this);
		UpdateNewIcon();
		DrawAnswers();
		this.OnPointerEnterAsObservable().Subscribe(delegate
		{
			OnPointerEnter();
		}).AddTo(this);
		this.OnPointerExitAsObservable().Subscribe(delegate
		{
			OnPointerExit();
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		DisposeAnswers();
	}

	private void DrawAnswers()
	{
		List<BlueprintCaseAnswer> list = UIUtilityDetective.GetAnswersWithTier(base.ViewModel.BlueprintCase).ToList();
		bool flag = UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.ShowAnswersAtCard);
		if (flag)
		{
			foreach (BlueprintCaseAnswer item in list)
			{
				BlueprintClue.UIType clueType = ((item.RelatedItem?.Blueprint is BlueprintClue clue) ? clue.GetUIData().UIType : BlueprintClue.UIType.Default);
				ExpandableAnswerView widget = WidgetFactory.GetWidget(m_AnswerPrefabs.GetEntity(clueType), activate: true, strictMatching: true);
				widget.transform.SetParent(m_AnswersContainer, worldPositionStays: false);
				widget.Bind(item);
				m_Answers.Add(widget);
			}
		}
		m_AnswersContainer.gameObject.SetActive(flag && !base.ViewModel.BlueprintCase.IsClosed());
	}

	private void DisposeAnswers()
	{
		m_Answers.ForEach(WidgetFactory.DisposeWidget);
		m_Answers.Clear();
	}

	public void UpdateNewIcon()
	{
		m_NewIconRp.Value = null;
		m_NewIcon.gameObject.SetActive(value: false);
	}

	private void OnPointerEnter()
	{
		if (!UIUtilityDetective.DebugFlags.HasFlag(AnswerDebugValues.AnimateAnswersLines) || base.ViewModel.BlueprintCase.IsClosed() || m_ViewsContext == null)
		{
			return;
		}
		ClearLines();
		foreach (DetectiveJournalClueView clue in m_ViewsContext.Clues)
		{
			if (!clue.ViewModel.IsAnswerClue.CurrentValue)
			{
				break;
			}
			(RectTransform, RectTransform) fromToDots = (ConnectionCenter, clue.ConnectionCenter);
			LineView lineView = Object.Instantiate(m_LinePrefab, m_ViewsContext.LinesContainer, worldPositionStays: false);
			lineView.Initialize(new ClueLineData(fromToDots), LineType.Answer);
			lineView.Animate();
			m_AnimateLines.Add(lineView);
		}
	}

	private void OnPointerExit()
	{
		ClearLines();
	}

	private void ClearLines()
	{
		m_AnimateLines.ForEach(delegate(LineView a)
		{
			a.Destroy();
		});
		m_AnimateLines.Clear();
	}
}
