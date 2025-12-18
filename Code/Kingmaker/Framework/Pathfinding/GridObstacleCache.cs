using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Pathfinding;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Pathfinding;
using Pathfinding.Util;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Rendering;

namespace Kingmaker.Framework.Pathfinding;

public sealed class GridObstacleCache : IEnumerable<GridObstacleCache.Entry>, IEnumerable, IDisposable
{
	public sealed class Entry
	{
		public readonly GridNodeIndex Node;

		public readonly GridNodeDirection Direction;

		public readonly HashSet<GridObstacle> Sources = new HashSet<GridObstacle>();

		public readonly GridConnectionIndex ConnectionIndex;

		public GridObstacle? Source;

		public LosCalculations.CoverType Type;

		public int Top;

		public int Bottom;

		public bool ZAligned;

		public int DirectionMask => 1 << (int)Direction;

		public bool Backward
		{
			get
			{
				GridNodeDirection direction;
				if (!ZAligned)
				{
					direction = Direction;
					return direction == GridNodeDirection.S || direction == GridNodeDirection.SW || direction == GridNodeDirection.SE;
				}
				direction = Direction;
				return direction == GridNodeDirection.W || direction == GridNodeDirection.NW || direction == GridNodeDirection.SW;
			}
		}

		public Entry(GridNodeIndex node, GridNodeDirection direction)
		{
			Node = node;
			Direction = direction;
			ConnectionIndex = new GridConnectionIndex(Node, Direction);
		}

		public void Clear()
		{
			Source = null;
			Type = LosCalculations.CoverType.Obstacle;
			Top = 0;
			Bottom = 0;
			ZAligned = false;
			Sources.Clear();
		}
	}

	private readonly Dictionary<GridObstacle, List<GridConnectionIndex>> _affectedConnections = new Dictionary<GridObstacle, List<GridConnectionIndex>>();

	private readonly GridGraph _graph;

	private Entry[] _connections;

	private NativeArray<int> _connectionCuts;

	private bool _dirty = true;

	public static GridObstacleCache? Instance => AdditionalGraphDataManager.Instance.GetGridDataOptional()?.Obstacles;

	public int Version { get; private set; }

	public NativeArray<int> ConnectionCuts
	{
		get
		{
			Update();
			return _connectionCuts;
		}
	}

	private IEnumerable<GridObstacle> Obstacles => NavmeshClipper.allEnabled.OfType<GridObstacle>();

	private GraphTransform GraphTransform => _graph.transform;

	public GridObstacleCache(GridGraph graph)
	{
		_graph = graph;
		int num = graph.width * graph.depth;
		_connections = new Entry[num * 8];
		_connectionCuts = new NativeArray<int>(num, Allocator.Persistent);
		InitConnections();
	}

	private void InitConnections()
	{
		for (int i = 0; i < _graph.width; i++)
		{
			for (int j = 0; j < _graph.depth; j++)
			{
				int num = i + j * _graph.width;
				for (int k = 0; k < 8; k++)
				{
					GridNodeDirection direction = (GridNodeDirection)k;
					_connections[num * 8 + k] = new Entry(new GridNodeIndex(i, j), direction);
				}
			}
		}
	}

	public Entry? GetObstacle(GridNodeIndex from, GridNodeIndex to)
	{
		return GetObstacle(from, from.GetDirection(to));
	}

	public Entry? GetObstacle(GridConnectionIndex connection)
	{
		return GetObstacle(connection.from, connection.direction);
	}

	public Entry? GetObstacle(GridNodeBase node, GridNodeDirection direction)
	{
		return GetObstacle(new GridNodeIndex(node.XCoordinateInGrid, node.ZCoordinateInGrid), direction);
	}

	public Entry? GetObstacle(GridNodeIndex node, GridNodeDirection direction)
	{
		Update();
		int num = (int)((node.x + node.z * _graph.width) * 8 + direction);
		Entry entry = _connections[num];
		if (!(entry.Source != null))
		{
			return null;
		}
		return entry;
	}

	public ReadonlyList<GridConnectionIndex> GetAffectedConnections(GridObstacle obstacle)
	{
		Update();
		return _affectedConnections.GetValueOrDefault(obstacle);
	}

	public void Invalidate()
	{
		_dirty = true;
	}

	public void ForceUpdate()
	{
		Update(force: true);
	}

	private void Update(bool force = false)
	{
		if (!_dirty && !force)
		{
			return;
		}
		using (ProfileScope.New("GridObstacleCache.Update"))
		{
			_affectedConnections.Clear();
			int num = _graph.width * _graph.depth;
			Entry[] connections;
			if (_connections.Length >= num * 8)
			{
				connections = _connections;
				for (int j = 0; j < connections.Length; j++)
				{
					connections[j].Clear();
				}
			}
			else
			{
				_connections = new Entry[num * 8];
				InitConnections();
			}
			if (_connectionCuts.Length >= num)
			{
				ref NativeArray<int> connectionCuts = ref _connectionCuts;
				int j = 0;
				connectionCuts.FillArray(in j, 0, _connectionCuts.Length);
			}
			else
			{
				_connectionCuts.Dispose();
				_connectionCuts = new NativeArray<int>(num, Allocator.Persistent);
			}
			foreach (GridObstacle item in Obstacles.Where((GridObstacle i) => (bool)i && i.enabled))
			{
				Add(item);
			}
			connections = _connections;
			foreach (Entry entry in connections)
			{
				Recalculate(entry);
			}
			_dirty = false;
			Version++;
			EventBus.RaiseEvent(delegate(IGridObstacleCacheHandler h)
			{
				h.HandleGridObstacleCacheUpdated();
			});
		}
	}

	private void Add(GridObstacle obstacle)
	{
		GridObstacle obstacle = obstacle;
		List<GridConnectionIndex> connections;
		using (ProfileScope.New("GridObstacleCollection.Update"))
		{
			List<GridConnectionIndex> list2 = (_affectedConnections[obstacle] = new List<GridConnectionIndex>(10));
			connections = list2;
			(GridNodeIndex forwardNode, GridNodeIndex backwardNode) affectedNodes = obstacle.GetAffectedNodes(GraphTransform);
			GridNodeIndex item = affectedNodes.forwardNode;
			GridNodeIndex item2 = affectedNodes.backwardNode;
			GridNodeIndex gridNodeIndex = (obstacle.ZAligned ? new GridNodeIndex(0, 1) : new GridNodeIndex(1, 0));
			AddConnection(item, item2);
			AddConnection(item2, item);
			AddConnection(item, item2 - gridNodeIndex);
			AddConnection(item2 - gridNodeIndex, item);
			AddConnection(item2, item - gridNodeIndex);
			AddConnection(item - gridNodeIndex, item2);
			AddConnection(item, item2 + gridNodeIndex);
			AddConnection(item2 + gridNodeIndex, item);
			AddConnection(item2, item + gridNodeIndex);
			AddConnection(item + gridNodeIndex, item2);
		}
		void AddConnection(GridNodeIndex from, GridNodeIndex to)
		{
			if (from.x >= 0 && from.x < _graph.width && from.z >= 0 && from.z < _graph.depth)
			{
				GridConnectionIndex item3 = new GridConnectionIndex(from, from.GetDirection(to));
				connections.Add(item3);
				_connections[(int)((from.x + from.z * _graph.width) * 8 + item3.direction)].Sources.Add(obstacle);
			}
		}
	}

	private unsafe void Recalculate(Entry entry)
	{
		if (entry.Node.x >= 0 && entry.Node.x < _graph.width && entry.Node.z >= 0 && entry.Node.z < _graph.depth)
		{
			int index = entry.Node.x + entry.Node.z * _graph.width;
			GetConnectionData(entry, out GridObstacle obstacle, out int top, out int bottom);
			entry.Source = obstacle;
			ref int reference = ref UnsafeUtility.ArrayElementAsRef<int>(_connectionCuts.GetUnsafePtr(), index);
			if (entry.Source == null)
			{
				reference &= ~entry.DirectionMask;
				entry.Clear();
				return;
			}
			entry.ZAligned = entry.Source.ZAligned;
			entry.Type = (entry.Backward ? entry.Source.TypeBackward : entry.Source.Type);
			entry.Top = top;
			entry.Bottom = bottom;
			reference = (entry.Source.KeepConnections ? (reference & ~entry.DirectionMask) : (reference | entry.DirectionMask));
		}
	}

	private static int GetPriority(GridConnectionIndex connection, GridObstacle obstacle)
	{
		int num;
		if (!obstacle.ZAligned)
		{
			GridNodeDirection direction = connection.direction;
			num = ((direction == GridNodeDirection.S || direction == GridNodeDirection.SW || direction == GridNodeDirection.SE) ? 1 : 0);
		}
		else
		{
			GridNodeDirection direction = connection.direction;
			num = ((direction == GridNodeDirection.W || direction == GridNodeDirection.NW || direction == GridNodeDirection.SW) ? 1 : 0);
		}
		int num2 = ((num != 0) ? obstacle.HeightBackward : obstacle.Height);
		return (int)(obstacle.transform.position.y * 1000f) + num2;
	}

	private static void GetConnectionData(Entry entry, out GridObstacle? obstacle, out int top, out int bottom)
	{
		if (entry.Sources.Count == 0)
		{
			obstacle = null;
			top = (bottom = 0);
			return;
		}
		int num = int.MinValue;
		obstacle = null;
		top = 0;
		bottom = int.MaxValue;
		foreach (GridObstacle source in entry.Sources)
		{
			int priority = GetPriority(entry.ConnectionIndex, source);
			if (priority > num)
			{
				num = priority;
				obstacle = source;
				int num2 = (int)(obstacle.transform.position.y * 1000f);
				top = num2 + (entry.Backward ? obstacle.HeightBackward : obstacle.Height);
				bottom = Math.Min(bottom, num2);
			}
		}
	}

	public IEnumerator<Entry> GetEnumerator()
	{
		Update();
		return _connections.Where((Entry i) => i.Source != null).GetEnumerator();
	}

	IEnumerator<Entry> IEnumerable<Entry>.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public void Dispose()
	{
		_connectionCuts.Dispose();
	}
}
