using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Gameplay.Features.AreaEffects.Shapes;
using Kingmaker.Gameplay.Features.Cohesion.Utility;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.BuildModeUtils;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Pathfinding;
using UnityEngine.Pool;

namespace Kingmaker.Gameplay.Utility;

public sealed class GridNodeToEntityCache : IDisposable
{
	[Flags]
	private enum ScoreFlags
	{
		Default = 1,
		NotMoving = 0x100,
		Conscious = 0x200,
		IsInCombat = 0x400,
		AreaEffect = 0x10000000,
		Destructible = 0x20000000,
		Unit = 0x40000000,
		Invalid = int.MinValue
	}

	private const int DefaultEntityListPoolCapacity = 2000;

	private const int DefaultEntityListCapacity = 8;

	private readonly ObjectPool<List<MechanicEntity>> _entityListPool = new ObjectPool<List<MechanicEntity>>(() => new List<MechanicEntity>(8), null, delegate(List<MechanicEntity> e)
	{
		e.Clear();
	}, null, collectionCheck: true, 2000);

	private readonly Dictionary<GridNodeIndex, List<MechanicEntity>> _nodeToEntities = new Dictionary<GridNodeIndex, List<MechanicEntity>>();

	private readonly Dictionary<MechanicEntity, NodeList> _entityToNodes = new Dictionary<MechanicEntity, NodeList>();

	public NodeList GetOccupiedNodes(MechanicEntity entity)
	{
		return _entityToNodes.GetValueOrDefault(entity);
	}

	public FilteredList<MechanicEntity> GetEntities(GridNodeIndex node)
	{
		return GetEntities<MechanicEntity>(node);
	}

	public FilteredList<T> GetEntities<T>(GridNodeIndex node, Func<T, bool>? pred = null) where T : MechanicEntity
	{
		return new FilteredList<T>(_nodeToEntities.GetValueOrDefault(node), pred);
	}

	public MechanicEntity? GetFirstEntity(GridNodeIndex node, Func<MechanicEntity, bool>? pred = null)
	{
		return this.GetFirstEntity<MechanicEntity>(node, pred);
	}

	public T? GetFirstEntity<T>(GridNodeIndex node, Func<T, bool>? pred = null) where T : MechanicEntity
	{
		List<MechanicEntity> valueOrDefault = _nodeToEntities.GetValueOrDefault(node);
		if (valueOrDefault == null)
		{
			return null;
		}
		foreach (MechanicEntity item in valueOrDefault)
		{
			if (item is T val && (pred == null || pred(val)))
			{
				return val;
			}
		}
		return null;
	}

	public bool ContainsEntity(GridNodeIndex node, MechanicEntity entity)
	{
		return _nodeToEntities.GetValueOrDefault(node).HasItem(entity);
	}

	public bool ContainsEntity(GridNodeIndex node, Func<MechanicEntity, bool>? pred = null)
	{
		return this.ContainsEntity<MechanicEntity>(node, pred);
	}

	public bool ContainsEntity<T>(GridNodeIndex node, Func<T, bool>? pred = null) where T : MechanicEntity
	{
		return GetEntities(node, pred).FirstItem() != null;
	}

	public void UpdateEntity(MechanicEntity entity)
	{
		using (ProfileScope.New("GridNodeToEntityCache.UpdateEntity"))
		{
			NodeList valueOrDefault = _entityToNodes.GetValueOrDefault(entity);
			foreach (GridNodeBase item in valueOrDefault)
			{
				RemoveEntityFromNode(entity, item);
			}
			ScoreFlags scoreFlags = Score(entity);
			if (((uint)scoreFlags & 0x80000000u) != 0)
			{
				valueOrDefault.Dispose();
				_entityToNodes.Remove(entity);
				return;
			}
			valueOrDefault = (_entityToNodes[entity] = GetOccupiedNodesNoCache(entity));
			foreach (GridNodeBase item2 in valueOrDefault)
			{
				AddEntityToNode(entity, item2, scoreFlags);
			}
			if (BuildModeUtility.IsDevelopment)
			{
				Validate(entity, valueOrDefault);
			}
		}
	}

	private void AddEntityToNode(MechanicEntity entity, GridNodeIndex node, ScoreFlags score)
	{
		List<MechanicEntity> list = _nodeToEntities.GetValueOrDefault(node);
		if (list == null)
		{
			list = (_nodeToEntities[node] = _entityListPool.Get());
		}
		for (int i = 0; i < list.Count; i++)
		{
			if (score > Score(list[i]))
			{
				list.Insert(i, entity);
				return;
			}
		}
		list.Add(entity);
	}

	private void RemoveEntityFromNode(MechanicEntity entity, GridNodeIndex node)
	{
		List<MechanicEntity> valueOrDefault = _nodeToEntities.GetValueOrDefault(node);
		if (valueOrDefault != null)
		{
			valueOrDefault.Remove(entity);
			if (valueOrDefault.Count == 0)
			{
				_nodeToEntities.Remove(node);
				_entityListPool.Release(valueOrDefault);
			}
		}
	}

	private static ScoreFlags Score(MechanicEntity entity)
	{
		if (entity.IsDisposed || !entity.IsInState || !entity.IsInGame)
		{
			return ScoreFlags.Invalid;
		}
		ScoreFlags scoreFlags = ScoreFlags.Default;
		if (entity is BaseUnitEntity baseUnitEntity)
		{
			scoreFlags |= ScoreFlags.Unit;
			if (baseUnitEntity.IsInCombat)
			{
				scoreFlags |= ScoreFlags.IsInCombat;
			}
			if (baseUnitEntity.IsConscious)
			{
				scoreFlags |= ScoreFlags.Conscious;
			}
			if (!IsMoving(baseUnitEntity))
			{
				scoreFlags |= ScoreFlags.NotMoving;
			}
		}
		else if (entity is DestructibleEntity)
		{
			scoreFlags |= ScoreFlags.Destructible;
		}
		else
		{
			if (!(entity is AreaEffectEntity { IsAllArea: false } areaEffectEntity) || areaEffectEntity.IsCohesionRange())
			{
				return ScoreFlags.Invalid;
			}
			scoreFlags |= ScoreFlags.AreaEffect;
		}
		return scoreFlags;
	}

	private static NodeList GetOccupiedNodesNoCache(MechanicEntity entity)
	{
		if (entity is AreaEffectEntity areaEffectEntity)
		{
			if (!(areaEffectEntity.Shape is AreaEffectShapePattern areaEffectShapePattern))
			{
				return NodeList.Empty;
			}
			return areaEffectShapePattern.CoveredNodes;
		}
		return entity.GetOccupiedNodes();
	}

	private static bool IsMovingOrDeadOrNotInCombat(BaseUnitEntity unit)
	{
		if (!IsMoving(unit) && !unit.IsDeadOrUnconscious)
		{
			return !unit.IsInCombat;
		}
		return true;
	}

	private static bool IsMoving(BaseUnitEntity unit)
	{
		return unit.GetOptional<PartMovable>()?.HasMotionThisSimulationTick ?? false;
	}

	public void Drop()
	{
		foreach (var (_, element) in _nodeToEntities)
		{
			_entityListPool.Release(element);
		}
		foreach (var (_, nodeList2) in _entityToNodes)
		{
			nodeList2.Dispose();
		}
		_nodeToEntities.Clear();
		_entityToNodes.Clear();
	}

	public void Dispose()
	{
		_entityListPool.Dispose();
	}

	private void Validate(MechanicEntity entity, NodeList nodes)
	{
		if (!(entity is BaseUnitEntity baseUnitEntity) || IsMovingOrDeadOrNotInCombat(baseUnitEntity))
		{
			return;
		}
		foreach (GridNodeBase item in nodes)
		{
			foreach (MechanicEntity item2 in _nodeToEntities.GetValueOrDefault(item))
			{
				if (item2 != baseUnitEntity && item2 is BaseUnitEntity baseUnitEntity2 && !IsMovingOrDeadOrNotInCombat(baseUnitEntity2))
				{
					PFLog.Default.ErrorWithReport("This units can't occupy the same node: ({0}, {1}), {2}, {3}", item.XCoordinateInGrid, item.ZCoordinateInGrid, baseUnitEntity, baseUnitEntity2);
				}
			}
		}
	}
}
