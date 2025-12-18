using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Framework.DetectiveSystem;
using Kingmaker.UI.Sound;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

[Serializable]
public class ClueDotsPositions
{
	[Header("Views")]
	[SerializeField]
	private DotWidget m_DotPrefab;

	[Header("Elements")]
	[SerializeField]
	private RectTransform m_TopPanel;

	[SerializeField]
	private RectTransform m_BottomPanel;

	[SerializeField]
	private RectTransform m_LeftPanel;

	[SerializeField]
	private RectTransform m_RightPanel;

	private DetectiveJournalClueView m_ParentClue;

	private readonly List<(BlueprintConclusion, DotWidget)> m_Dots = new List<(BlueprintConclusion, DotWidget)>();

	public void Initialize(DetectiveJournalClueView parentClue)
	{
		m_ParentClue = parentClue;
	}

	public void UpdateState()
	{
		m_Dots.ForEach(delegate((BlueprintConclusion, DotWidget) d)
		{
			d.Item2.SetState(State(d.Item1));
		});
	}

	private DotState State(BlueprintConclusion conclusion)
	{
		bool flag = UIConfig.Instance.DetectiveConfig.AnswerDebugValues.HasFlag(AnswerDebugValues.ChangeDotColor);
		if (conclusion.IsRefuted())
		{
			return DotState.Refuted;
		}
		if (!((m_ParentClue?.ViewModel?.IsAnswerClue.CurrentValue).GetValueOrDefault() && flag))
		{
			return DotState.Default;
		}
		return DotState.Answer;
	}

	public void DestroyDotsFor(BlueprintConclusion conclusion)
	{
		foreach (var item in m_Dots.Where(((BlueprintConclusion, DotWidget) kvp) => kvp.Item1 == conclusion).ToList())
		{
			if (item.Item2 != null)
			{
				m_Dots.Remove(item);
			}
			WidgetFactory.DisposeWidget(item.Item2);
		}
		UpdatePanelsVisual();
	}

	public LineDirection GetClosestDirectionTo(Vector3 position, RectTransform parent)
	{
		return new List<(RectTransform, LineDirection)>
		{
			(m_BottomPanel, LineDirection.Down),
			(m_LeftPanel, LineDirection.Left),
			(m_RightPanel, LineDirection.Right),
			(m_TopPanel, LineDirection.Up)
		}.OrderBy(((RectTransform, LineDirection) kvp) => (!(kvp.Item1 != null)) ? float.MaxValue : Vector3.Distance(parent.InverseTransformPoint(kvp.Item1.position) + kvp.Item2.GetDirectionVector() * 100f, position)).FirstOrDefault().Item2;
	}

	public DotWidget GetDotByDirection(BlueprintConclusion conclusion, LineDirection direction)
	{
		List<(BlueprintConclusion, DotWidget)> list = m_Dots.Where(((BlueprintConclusion, DotWidget) kvp) => kvp.Item1 == conclusion).ToList();
		DotWidget dotWidget = null;
		RectTransform panelByDirection = GetPanelByDirection(direction);
		foreach (var item in list)
		{
			if (item.Item2 != null && item.Item2.transform.parent != panelByDirection)
			{
				WidgetFactory.DisposeWidget(item.Item2);
				m_Dots.Remove(item);
			}
			else
			{
				dotWidget = item.Item2;
			}
		}
		if (dotWidget != null)
		{
			return dotWidget;
		}
		DotWidget widget = WidgetFactory.GetWidget(m_DotPrefab);
		widget.transform.SetParent(panelByDirection, worldPositionStays: false);
		m_Dots.Add((conclusion, widget));
		widget.SetState(State(conclusion));
		UISounds.Instance.Sounds.DetectiveSystem.SetLineTargetDot.Play();
		UpdatePanelsVisual();
		return widget;
	}

	private void UpdatePanelsVisual()
	{
		UpdateAlpha(m_TopPanel);
		UpdateAlpha(m_BottomPanel);
		UpdateAlpha(m_LeftPanel);
		UpdateAlpha(m_RightPanel);
		void UpdateAlpha(RectTransform panel)
		{
			if (!(panel == null))
			{
				int num = m_Dots.Count(((BlueprintConclusion, DotWidget) kvp) => kvp.Item2.transform.parent == panel);
				panel.EnsureComponent<CanvasGroup>().alpha = ((num > 0) ? 1 : 0);
			}
		}
	}

	private RectTransform GetPanelByDirection(LineDirection direction)
	{
		return direction switch
		{
			LineDirection.None => m_BottomPanel, 
			LineDirection.Left => m_LeftPanel, 
			LineDirection.Right => m_RightPanel, 
			LineDirection.Up => m_TopPanel, 
			LineDirection.Down => m_BottomPanel, 
			_ => throw new ArgumentOutOfRangeException("direction", direction, null), 
		};
	}
}
