using System.Collections.Generic;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[TypeId("da006e6d97ab3d14c8f0fdf4616fc61d")]
public class ContextActionRunToTarget : ContextAction
{
	public int maxRunDistance;

	public bool storeStartingPosition;

	public bool runToStoredPosition;

	public ActionList ActionsOnSuccess;

	public override string GetCaption()
	{
		return ((!runToStoredPosition) ? "Caster run to position close to target" : "Caster run to saved position") + " by straight line" + (storeStartingPosition ? " and starting position is stored" : "");
	}

	protected override void RunAction()
	{
		MechanicEntity caster = base.Context.Owner;
		MechanicEntity entity = base.Target.Entity;
		if (caster == null || entity == null)
		{
			return;
		}
		List<Vector3> list = new List<Vector3>();
		if (runToStoredPosition)
		{
			WarhammerUnitPartStorePosition warhammerUnitPartStorePosition = caster?.GetOptional<WarhammerUnitPartStorePosition>();
			if (warhammerUnitPartStorePosition == null)
			{
				Element.LogError(this, "WarhammerContextActionRunToTarget: no stored position");
				return;
			}
			list.Add(warhammerUnitPartStorePosition.storedPosition);
		}
		else
		{
			for (int i = -1; i <= 1; i++)
			{
				for (int j = -1; j <= 1; j++)
				{
					Vector3 vector = entity.Position + new Vector3(GraphParamsMechanicsCache.GridCellSize * (float)i, 0f, GraphParamsMechanicsCache.GridCellSize * (float)j);
					if (CheckRunPath(caster, vector))
					{
						list.Add(vector);
					}
				}
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		if (storeStartingPosition)
		{
			caster.GetOrCreate<WarhammerUnitPartStorePosition>().storedPosition = caster.Position;
		}
		list.Sort((Vector3 a, Vector3 b) => (int)Mathf.Sign((a - caster.Position).sqrMagnitude - (b - caster.Position).sqrMagnitude));
		caster.Position = list.FirstItem();
		using (base.Context.PushTarget(entity))
		{
			ActionsOnSuccess.Run();
		}
	}

	private bool CheckRunPath(MechanicEntity caster, Vector3 point)
	{
		NodeList nodes = GetOrientedPattern(caster.Position, point).Nodes;
		PriorityQueue<GridNodeBase> priorityQueue = new PriorityQueue<GridNodeBase>(DistanceComparer(caster), EqualityComparer<GridNodeBase>.Default);
		foreach (GridNodeBase item in nodes)
		{
			priorityQueue.Enqueue(item);
		}
		Dictionary<GridNodeBase, BaseUnitEntity> dictionary = new Dictionary<GridNodeBase, BaseUnitEntity>();
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			dictionary[(GridNodeBase)(GraphNode)allBaseUnit.CurrentNode] = allBaseUnit;
		}
		GridNodeBase gridNodeBase = (GridNodeBase)(GraphNode)ObstacleAnalyzer.GetNearestNode(point);
		if (dictionary.ContainsKey(gridNodeBase))
		{
			return false;
		}
		GridNodeBase gridNodeBase2 = (GridNodeBase)(GraphNode)caster.CurrentNode;
		while (priorityQueue.Count > 0)
		{
			if (dictionary.TryGetValue(gridNodeBase2, out var value) && value.CombatGroup.IsEnemy(caster))
			{
				return false;
			}
			if (gridNodeBase2 == gridNodeBase)
			{
				return true;
			}
			GridNodeBase gridNodeBase3 = priorityQueue.Dequeue();
			if (!gridNodeBase2.ContainsConnection(gridNodeBase3))
			{
				return false;
			}
			gridNodeBase2 = gridNodeBase3;
		}
		return gridNodeBase2 == gridNodeBase;
	}

	public IComparer<GridNodeBase> DistanceComparer(MechanicEntity caster)
	{
		return Comparer<GridNodeBase>.Create((GridNodeBase a, GridNodeBase b) => Comparer<float>.Default.Compare(caster.DistanceToInCells(a.Vector3Position()), caster.DistanceToInCells(b.Vector3Position())));
	}

	public OrientedPatternData GetOrientedPattern(Vector3 casterPos, Vector3 targetPos)
	{
		return GetOrientedRayPattern(casterPos, targetPos, maxRunDistance);
	}

	private OrientedPatternData GetOrientedRayPattern(Vector3 casterPos, Vector3 targetPos, int length)
	{
		GridNodeBase actualCastNode = AoEPatternHelper.GetActualCastNode(base.Context.Caster, casterPos, targetPos, 1, 1);
		return AoEPattern.Ray(length).GetOriented(actualCastNode, targetPos - casterPos);
	}
}
