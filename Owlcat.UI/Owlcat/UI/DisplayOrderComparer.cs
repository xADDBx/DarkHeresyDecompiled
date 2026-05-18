using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.UI;

internal class DisplayOrderComparer : IComparer<Transform>
{
	public static readonly DisplayOrderComparer Comparer = new DisplayOrderComparer();

	private readonly List<int> m_Trace1 = new List<int>();

	private readonly List<int> m_Trace2 = new List<int>();

	private DisplayOrderComparer()
	{
	}

	public int Compare(Transform x, Transform y)
	{
		int num = CompareCanvas(GetCanvas(x), GetCanvas(y));
		if (num != 0)
		{
			return num;
		}
		GetTrace(x, m_Trace1);
		GetTrace(y, m_Trace2);
		return CompareTrace(m_Trace1, m_Trace2);
	}

	internal static Canvas GetCanvas(Transform transform)
	{
		Canvas component = null;
		while (transform != null)
		{
			if (transform.TryGetComponent<Canvas>(out component))
			{
				return component;
			}
			transform = transform.parent;
		}
		return component;
	}

	internal static void GetTrace(Transform transform, List<int> result)
	{
		result.Clear();
		while (transform != null)
		{
			result.Add(transform.GetSiblingIndex());
			transform = transform.parent;
		}
		result.Reverse();
	}

	private static int CompareCanvas(Canvas canvas1, Canvas canvas2)
	{
		if (canvas1 == null || canvas2 == null)
		{
			return 0;
		}
		return canvas1.sortingOrder.CompareTo(canvas2.sortingOrder);
	}

	private static int CompareTrace(List<int> trace1, List<int> trace2)
	{
		int num = Mathf.Min(trace1.Count, trace2.Count);
		for (int i = 0; i < num; i++)
		{
			int num2 = trace1[i].CompareTo(trace2[i]);
			if (num2 != 0)
			{
				return num2;
			}
		}
		return trace1.Count.CompareTo(trace2.Count);
	}
}
