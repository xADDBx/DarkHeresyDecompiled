using System;
using System.Collections;
using System.Collections.Generic;
using Kingmaker.Framework.Pathfinding;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.Pathfinding;

public readonly struct NodeList : IEnumerable<GridNodeBase>, IEnumerable, IDisposable
{
	public struct Enumerator : IEnumerator<GridNodeBase>, IEnumerator, IDisposable
	{
		private readonly GridGraph m_Graph;

		private PatternGridData.Enumerator m_PatternEnumerator;

		private GridNodeBase m_Current;

		public GridNodeBase Current => m_Current;

		object IEnumerator.Current => Current;

		public Enumerator(GridGraph graph, PatternGridData.Enumerator enumerator)
		{
			this = default(Enumerator);
			m_Graph = graph;
			m_PatternEnumerator = enumerator;
			Reset();
		}

		public bool MoveNext()
		{
			do
			{
				if (!m_PatternEnumerator.MoveNext())
				{
					return false;
				}
				m_Current = m_Graph.GetNode(m_PatternEnumerator.Current.x, m_PatternEnumerator.Current.y);
			}
			while (m_Current == null);
			return true;
		}

		public void Reset()
		{
			m_PatternEnumerator.Reset();
			m_Current = null;
		}

		public void Dispose()
		{
			m_PatternEnumerator.Dispose();
		}
	}

	public static readonly NodeList Empty = new NodeList(null, in PatternGridData.Empty);

	private readonly GridGraph m_Graph;

	private readonly PatternGridData m_Pattern;

	public bool IsEmpty => m_Pattern.IsEmpty;

	public NodeList(GridGraph graph, in PatternGridData pattern)
	{
		m_Graph = graph;
		m_Pattern = pattern;
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(m_Graph, m_Pattern.GetEnumerator());
	}

	IEnumerator<GridNodeBase> IEnumerable<GridNodeBase>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public bool Contains(GridNodeBase item)
	{
		if (item.Graph == m_Graph)
		{
			return m_Pattern.Contains(item.CoordinatesInGrid);
		}
		return false;
	}

	public bool ContainsExcluded(GridNodeBase item)
	{
		if (item.Graph == m_Graph)
		{
			return m_Pattern.ContainsExcluded(item.CoordinatesInGrid);
		}
		return false;
	}

	public bool ContainsAny(GridNodeBase item)
	{
		if (item.Graph == m_Graph)
		{
			return m_Pattern.ContainsAny(item.CoordinatesInGrid);
		}
		return false;
	}

	public bool Contains(GridNodeIndex index)
	{
		return m_Pattern.Contains(new Vector2Int(index.x, index.z));
	}

	public bool ContainsExcluded(GridNodeIndex index)
	{
		return m_Pattern.Contains(new Vector2Int(index.x, index.z));
	}

	public bool ContainsAny(GridNodeIndex index)
	{
		return m_Pattern.Contains(new Vector2Int(index.x, index.z));
	}

	public void Dispose()
	{
		m_Pattern.Dispose();
	}
}
