using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.Common;

public sealed class SegmentedProgressBar : BaseProgressBar<int>
{
	private enum Direction
	{
		LeftToRight,
		RightToLeft
	}

	[SerializeField]
	private BaseProgressBarSegment m_SegmentPrefab;

	[SerializeField]
	private Transform m_SegmentsParent;

	[SerializeField]
	private Direction m_Direction;

	private List<BaseProgressBarSegment> m_Segments = new List<BaseProgressBarSegment>();

	private int m_Min;

	private int m_Max;

	private int m_CurrentValue;

	public override void SetLimits(int min, int max)
	{
		m_Min = min;
		m_Max = max;
	}

	public override void SetValue(int value)
	{
		m_CurrentValue = Mathf.Clamp(value, m_Min, m_Max);
		UpdateSegments();
	}

	private void UpdateSegments()
	{
		int segmentCount = Mathf.Max(0, m_Max - m_Min);
		if (m_Direction == Direction.LeftToRight)
		{
			UpdateLeftToRight(segmentCount);
		}
		else
		{
			UpdateRightToLeft(segmentCount);
		}
	}

	private void UpdateLeftToRight(int segmentCount)
	{
		bool isMaxValue = m_CurrentValue == m_Max;
		for (int i = 0; i < segmentCount; i++)
		{
			BaseProgressBarSegment segment = GetSegment(i);
			segment.SetFill(m_CurrentValue > m_Min + i, isMaxValue);
			segment.gameObject.SetActive(value: true);
		}
		for (int j = segmentCount; j < m_Segments.Count; j++)
		{
			m_Segments[j].gameObject.SetActive(value: false);
		}
	}

	private void UpdateRightToLeft(int segmentCount)
	{
		bool isMaxValue = m_CurrentValue == m_Max;
		for (int num = segmentCount - 1; num >= 0; num--)
		{
			BaseProgressBarSegment segment = GetSegment(num);
			segment.SetFill(segmentCount - m_CurrentValue <= m_Min + num, isMaxValue);
			segment.gameObject.SetActive(value: true);
		}
		for (int i = segmentCount; i < m_Segments.Count; i++)
		{
			m_Segments[i].gameObject.SetActive(value: false);
		}
	}

	private BaseProgressBarSegment GetSegment(int idx)
	{
		while (m_Segments.Count <= idx)
		{
			BaseProgressBarSegment baseProgressBarSegment = Object.Instantiate(m_SegmentPrefab, m_SegmentsParent);
			baseProgressBarSegment.gameObject.SetActive(value: false);
			m_Segments.Add(baseProgressBarSegment);
		}
		return m_Segments[idx];
	}
}
