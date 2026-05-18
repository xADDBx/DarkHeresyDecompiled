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
using UnityEngine.Pool;
using UnityEngine.Rendering;

namespace Kingmaker.Framework.Pathfinding;

public sealed class GridObstacleCache : IEnumerable<GridObstacleCache.Entry>, IEnumerable, IDisposable
{
	public readonly struct Entry
	{
		public readonly GridConnectionIndex ConnectionIndex;

		public readonly GridObstacle? Source;

		public readonly LosCalculations.CoverType Type;

		public readonly int Top;

		public readonly int Bottom;

		public readonly bool ZAligned;

		public GridNodeIndex Node => ConnectionIndex.from;

		public GridNodeDirection Direction => ConnectionIndex.direction;

		public bool Exists => (object)Source != null;

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

		public ReadonlyList<GridObstacle> Sources => Instance?.GetSourcesForConnection(ConnectionIndex.from, ConnectionIndex.direction) ?? ReadonlyList<GridObstacle>.Empty;

		public Entry(GridNodeIndex node, GridNodeDirection direction)
			: this(node, direction, null, LosCalculations.CoverType.Obstacle, 0, 0, zAligned: false)
		{
		}

		public Entry(GridNodeIndex node, GridNodeDirection direction, GridObstacle? source, LosCalculations.CoverType type, int top, int bottom, bool zAligned)
		{
			ConnectionIndex = new GridConnectionIndex(node, direction);
			Source = source;
			Type = type;
			Top = top;
			Bottom = bottom;
			ZAligned = zAligned;
		}

		public Entry Cleared()
		{
			return new Entry(Node, Direction);
		}
	}

	private readonly Dictionary<GridObstacle, List<GridConnectionIndex>> _obstacleToConnections = new Dictionary<GridObstacle, List<GridConnectionIndex>>();

	private readonly Dictionary<int, List<GridObstacle>> _connectionToObstacles = new Dictionary<int, List<GridObstacle>>();

	private readonly GridGraph _graph;

	private readonly int _width;

	private readonly int _depth;

	private Entry[] _connections;

	private NativeArray<int> _connectionCuts;

	private bool _allDirty = true;

	private readonly HashSet<GridObstacle> _dirtyObstacles = new HashSet<GridObstacle>();

	public static GridObstacleCache? Instance
	{
		get
		{
			GridObstacleCache obj = AdditionalGraphDataManager.Instance.GetGridDataOptional()?.Obstacles;
			if (obj != null)
			{
				obj.Update();
				return obj;
			}
			return obj;
		}
	}

	public long Version { get; private set; }

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
		_width = graph.width;
		_depth = graph.depth;
		int num = _width * _depth;
		_connections = new Entry[num * 8];
		_connectionCuts = new NativeArray<int>(num, Allocator.Persistent);
		InitConnections();
	}

	private void InitConnections()
	{
		for (int i = 0; i < _width; i++)
		{
			for (int j = 0; j < _depth; j++)
			{
				int num = i + j * _width;
				for (int k = 0; k < 8; k++)
				{
					GridNodeDirection direction = (GridNodeDirection)k;
					_connections[num * 8 + k] = new Entry(new GridNodeIndex(i, j), direction);
				}
			}
		}
	}

	public Entry GetObstacle(GridNodeIndex from, GridNodeIndex to)
	{
		return GetObstacle(from, from.GetDirection(to));
	}

	public Entry GetObstacle(GridConnectionIndex connection)
	{
		return GetObstacle(connection.from, connection.direction);
	}

	public Entry GetObstacle(GridNodeBase node, GridNodeDirection direction)
	{
		return GetObstacle(new GridNodeIndex(node.XCoordinateInGrid, node.ZCoordinateInGrid), direction);
	}

	private Entry GetObstacle(GridNodeIndex node, GridNodeDirection direction)
	{
		Update();
		int num = (int)((node.x + node.z * _width) * 8 + direction);
		Entry result = _connections[num];
		if (!(result.Source != null))
		{
			return default(Entry);
		}
		return result;
	}

	public ReadonlyList<GridConnectionIndex> GetAffectedConnections(GridObstacle obstacle)
	{
		Update();
		return _obstacleToConnections.GetValueOrDefault(obstacle);
	}

	private ReadonlyList<GridObstacle> GetSourcesForConnection(GridNodeIndex node, GridNodeDirection direction)
	{
		return _connectionToObstacles.GetValueOrDefault(GetConnectionIndex(node, direction));
	}

	public void Invalidate()
	{
		_allDirty = true;
	}

	public void ForceUpdate()
	{
		Update(force: true);
	}

	public void OnObstacleChanged(GridObstacle obstacle)
	{
		_dirtyObstacles.Add(obstacle);
	}

	private void Update(bool force = false)
	{
		using (ProfileScope.NewScope("Update"))
		{
			if (_allDirty || force)
			{
				RecalculateAll();
			}
			else
			{
				if (_dirtyObstacles.Count == 0)
				{
					return;
				}
				bool flag = false;
				try
				{
					foreach (GridObstacle dirtyObstacle in _dirtyObstacles)
					{
						bool flag2 = dirtyObstacle != null && dirtyObstacle.gameObject.activeInHierarchy && dirtyObstacle.enabled;
						bool flag3 = _obstacleToConnections.ContainsKey(dirtyObstacle);
						if (flag2 && !flag3)
						{
							AddObstacle(dirtyObstacle);
							flag = true;
						}
						else if (!flag2 && flag3)
						{
							RemoveObstacle(dirtyObstacle);
							flag = true;
						}
						else if (flag2 && flag3)
						{
							UpdateObstacle(dirtyObstacle);
							flag = true;
						}
					}
				}
				finally
				{
					_dirtyObstacles.Clear();
				}
				if (flag)
				{
					Version++;
					EventBus.RaiseEvent(delegate(IGridObstacleCacheHandler h)
					{
						h.HandleGridObstacleCacheUpdated();
					});
				}
			}
		}
	}

	private void RecalculateAll()
	{
		using (ProfileScope.New("GridObstacleCache.Update"))
		{
			_obstacleToConnections.Clear();
			_connectionToObstacles.Clear();
			_dirtyObstacles.Clear();
			int num = _width * _depth;
			int num2 = num * 8;
			if (_connections.Length < num2)
			{
				_connections = new Entry[num2];
				InitConnections();
			}
			else
			{
				for (int i = 0; i < num2; i++)
				{
					_connections[i] = _connections[i].Cleared();
				}
			}
			if (_connectionCuts.Length < num)
			{
				_connectionCuts.Dispose();
				_connectionCuts = new NativeArray<int>(num, Allocator.Persistent);
			}
			else
			{
				ref NativeArray<int> connectionCuts = ref _connectionCuts;
				int value = 0;
				connectionCuts.FillArray(in value, 0, num);
			}
			foreach (GridObstacle obstacle in Obstacles)
			{
				if (obstacle != null && obstacle.gameObject.activeInHierarchy && obstacle.enabled)
				{
					AddObstacle(obstacle);
				}
			}
			for (int j = 0; j < num2; j++)
			{
				RecalculateConnectionData(j);
			}
			_allDirty = false;
			Version++;
			EventBus.RaiseEvent(delegate(IGridObstacleCacheHandler h)
			{
				h.HandleGridObstacleCacheUpdated();
			});
		}
	}

	private void AddObstacle(GridObstacle obstacle)
	{
		GridObstacle obstacle = obstacle;
		List<GridConnectionIndex> connections = new List<GridConnectionIndex>(10);
		_obstacleToConnections[obstacle] = connections;
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
		Version++;
		void AddConnection(GridNodeIndex from, GridNodeIndex to)
		{
			if (from.x >= 0 && from.x < _width && from.z >= 0 && from.z < _depth)
			{
				GridConnectionIndex item3 = new GridConnectionIndex(from, from.GetDirection(to));
				int connectionIndex = GetConnectionIndex(from, item3.direction);
				connections.Add(item3);
				if (!_connectionToObstacles.TryGetValue(connectionIndex, out List<GridObstacle> value))
				{
					value = new List<GridObstacle>(2);
					_connectionToObstacles[connectionIndex] = value;
				}
				value.Add(obstacle);
			}
		}
	}

	private void RemoveObstacle(GridObstacle obstacle)
	{
		if (!_obstacleToConnections.Remove(obstacle, out List<GridConnectionIndex> value))
		{
			return;
		}
		foreach (GridConnectionIndex item in value)
		{
			int connectionIndex = GetConnectionIndex(item.from, item.direction);
			if (_connectionToObstacles.TryGetValue(connectionIndex, out List<GridObstacle> value2))
			{
				value2.Remove(obstacle);
				if (value2.Count == 0)
				{
					_connectionToObstacles.Remove(connectionIndex);
				}
			}
			RecalculateConnectionData(connectionIndex);
		}
		Version++;
	}

	private void UpdateObstacle(GridObstacle obstacle)
	{
		if (!_obstacleToConnections.TryGetValue(obstacle, out List<GridConnectionIndex> value))
		{
			return;
		}
		HashSet<int> value2;
		using (CollectionPool<HashSet<int>, int>.Get(out value2))
		{
			foreach (GridConnectionIndex item in value)
			{
				value2.Add(GetConnectionIndex(item.from, item.direction));
			}
			_obstacleToConnections.Remove(obstacle);
			foreach (int item2 in value2)
			{
				if (_connectionToObstacles.TryGetValue(item2, out List<GridObstacle> value3))
				{
					value3.Remove(obstacle);
					if (value3.Count == 0)
					{
						_connectionToObstacles.Remove(item2);
					}
				}
			}
			AddObstacle(obstacle);
			List<GridConnectionIndex> valueOrDefault = _obstacleToConnections.GetValueOrDefault(obstacle);
			if (valueOrDefault != null)
			{
				foreach (GridConnectionIndex item3 in valueOrDefault)
				{
					value2.Add(GetConnectionIndex(item3.from, item3.direction));
				}
			}
			foreach (int item4 in value2)
			{
				RecalculateConnectionData(item4);
			}
		}
	}

	private void RecalculateConnectionData(int connIndex)
	{
		if (!_connectionToObstacles.TryGetValue(connIndex, out List<GridObstacle> value) || value.Count == 0)
		{
			ClearConnectionData(connIndex);
			return;
		}
		GridObstacle gridObstacle = null;
		int num = int.MinValue;
		ref Entry reference = ref _connections[connIndex];
		foreach (GridObstacle item in value)
		{
			if (!(item == null))
			{
				int priority = GetPriority(reference.ConnectionIndex, item);
				if (priority > num)
				{
					num = priority;
					gridObstacle = item;
				}
			}
		}
		if (gridObstacle == null)
		{
			ClearConnectionData(connIndex);
			return;
		}
		GridObstacle source = gridObstacle;
		bool zAligned = gridObstacle.ZAligned;
		LosCalculations.CoverType type = (reference.Backward ? gridObstacle.TypeBackward : gridObstacle.Type);
		int num2 = (int)(gridObstacle.transform.position.y * 1000f);
		int top = num2 + (reference.Backward ? gridObstacle.HeightBackward : gridObstacle.Height);
		int bottom = num2;
		_connections[connIndex] = new Entry(reference.Node, reference.Direction, source, type, top, bottom, zAligned);
		UpdateConnectionCut(connIndex, gridObstacle.KeepConnections);
	}

	private unsafe void ClearConnectionData(int connIndex)
	{
		_connections[connIndex] = _connections[connIndex].Cleared();
		int index = connIndex / 8;
		int num = 1 << connIndex % 8;
		UnsafeUtility.ArrayElementAsRef<int>(_connectionCuts.GetUnsafePtr(), index) &= ~num;
	}

	private unsafe void UpdateConnectionCut(int connIndex, bool keepConnections)
	{
		int index = connIndex / 8;
		int num = 1 << connIndex % 8;
		ref int reference = ref UnsafeUtility.ArrayElementAsRef<int>(_connectionCuts.GetUnsafePtr(), index);
		if (keepConnections)
		{
			reference &= ~num;
		}
		else
		{
			reference |= num;
		}
	}

	private int GetConnectionIndex(GridNodeIndex node, GridNodeDirection direction)
	{
		return (int)((node.x + node.z * _width) * 8 + direction);
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
		if (_connectionCuts.IsCreated)
		{
			_connectionCuts.Dispose();
		}
	}
}
