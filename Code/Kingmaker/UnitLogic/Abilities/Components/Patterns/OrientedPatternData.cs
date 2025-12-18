using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;

namespace Kingmaker.UnitLogic.Abilities.Components.Patterns;

public struct OrientedPatternData
{
	public readonly struct NodesWithExtraDataEnumerable
	{
		private readonly NodeList m_NodeList;

		private readonly Dictionary<GridNodeBase, PatternCellData> m_NodesExtraData;

		public NodesWithExtraDataEnumerable(NodeList nodeList, Dictionary<GridNodeBase, PatternCellData> nodesExtraData)
		{
			m_NodeList = nodeList;
			m_NodesExtraData = nodesExtraData;
		}

		public NodesWithExtraDataEnumerator GetEnumerator()
		{
			return new NodesWithExtraDataEnumerator(m_NodeList.GetEnumerator(), m_NodesExtraData);
		}
	}

	public struct NodesWithExtraDataEnumerator
	{
		private NodeList.Enumerator m_NodesEnumerator;

		private readonly Dictionary<GridNodeBase, PatternCellData> m_NodesExtraData;

		private (GridNodeBase node, PatternCellData patternCellData) m_Current;

		public (GridNodeBase node, PatternCellData patternCellData) Current => m_Current;

		public NodesWithExtraDataEnumerator(NodeList.Enumerator nodesEnumerator, Dictionary<GridNodeBase, PatternCellData> nodesExtraData)
		{
			m_NodesEnumerator = nodesEnumerator;
			m_NodesExtraData = nodesExtraData;
			m_Current = default((GridNodeBase, PatternCellData));
		}

		public bool MoveNext()
		{
			if (m_NodesEnumerator.MoveNext())
			{
				if (m_NodesExtraData != null && m_NodesExtraData.TryGetValue(m_NodesEnumerator.Current, out var value))
				{
					m_Current = (node: m_NodesEnumerator.Current, patternCellData: value);
				}
				else
				{
					m_Current = (node: m_NodesEnumerator.Current, patternCellData: PatternCellData.Empty);
				}
				return true;
			}
			return false;
		}
	}

	public static readonly OrientedPatternData Empty = new OrientedPatternData(new List<GridNodeBase>(), null);

	[CanBeNull]
	private readonly Dictionary<GridNodeBase, PatternCellData> m_NodesExtraData;

	private NodeList m_Nodes;

	[CanBeNull]
	public GridNodeBase ApplicationNode { get; }

	public NodeList Nodes => m_Nodes;

	public NodesWithExtraDataEnumerable NodesWithExtraData => new NodesWithExtraDataEnumerable(Nodes, m_NodesExtraData);

	public OrientedPatternData([NotNull] Dictionary<GridNodeBase, PatternCellDataAccumulator> nodes, [NotNull] GridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		PatternGridData pattern = PatternGridData.Create(nodes.Keys.Select((GridNodeBase v) => v.CoordinatesInGrid).ToTempHashSet(), disposable: true);
		m_Nodes = new NodeList((GridGraph)applicationNode.Graph, in pattern);
		m_NodesExtraData = new Dictionary<GridNodeBase, PatternCellData>(nodes.Count);
		foreach (KeyValuePair<GridNodeBase, PatternCellDataAccumulator> node in nodes)
		{
			m_NodesExtraData.Add(node.Key, node.Value.Result);
		}
	}

	public OrientedPatternData([NotNull] List<GridNodeBase> nodes, GridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		m_NodesExtraData = null;
		if (nodes.Count == 0)
		{
			m_Nodes = NodeList.Empty;
			return;
		}
		GridGraph graph = (GridGraph)nodes[0].Graph;
		PatternGridData pattern = PatternGridData.Create(nodes.Select((GridNodeBase v) => v.CoordinatesInGrid).ToTempHashSet(), disposable: true);
		m_Nodes = new NodeList(graph, in pattern);
	}

	public OrientedPatternData([NotNull] HashSet<GridNodeBase> nodes, GridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		m_NodesExtraData = null;
		if (nodes.Count == 0)
		{
			m_Nodes = NodeList.Empty;
			return;
		}
		GridGraph graph = (GridGraph)nodes.First().Graph;
		PatternGridData pattern = PatternGridData.Create(nodes.Select((GridNodeBase v) => v.CoordinatesInGrid).ToTempHashSet(), disposable: true);
		m_Nodes = new NodeList(graph, in pattern);
	}

	public OrientedPatternData(PatternGridData data, GridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		m_NodesExtraData = null;
		GridGraph graph = (GridGraph)applicationNode.Graph;
		m_Nodes = new NodeList(graph, in data);
	}

	public OrientedPatternData(in NodeList nodes, GridNodeBase applicationNode)
	{
		ApplicationNode = applicationNode;
		m_NodesExtraData = null;
		m_Nodes = nodes;
	}

	public bool Contains(GridNodeBase node)
	{
		return m_Nodes.Contains(node);
	}

	public bool ContainsExcluded(GridNodeBase node)
	{
		return m_Nodes.ContainsExcluded(node);
	}

	public bool ContainsAny(GridNodeBase node)
	{
		return m_Nodes.ContainsAny(node);
	}

	public bool Contains(GridNodeIndex gridNodeIndex)
	{
		return m_Nodes.Contains(gridNodeIndex);
	}

	public bool ContainsExcluded(GridNodeIndex gridNodeIndex)
	{
		return m_Nodes.ContainsExcluded(gridNodeIndex);
	}

	public bool ContainsAny(GridNodeIndex gridNodeIndex)
	{
		return m_Nodes.ContainsAny(gridNodeIndex);
	}

	public bool TryGet(GridNodeBase node, out PatternCellData data)
	{
		if (!m_Nodes.Contains(node))
		{
			data = PatternCellData.Empty;
			return false;
		}
		if (m_NodesExtraData == null || !m_NodesExtraData.TryGetValue(node, out data))
		{
			data = PatternCellData.Empty;
		}
		return true;
	}
}
