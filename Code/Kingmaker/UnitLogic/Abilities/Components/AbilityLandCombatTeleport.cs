using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.Pathfinding;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[TypeId("8de79e536f4d4955ada731479ecfa196")]
public class AbilityLandCombatTeleport : AbilityCustomLogic, IAbilityTargetRestriction
{
	protected static readonly TimeSpan MaxTeleportationDuration = 2.5f.Seconds();

	public TeleportationType TeleportationType;

	[ShowIf("IsMoveSelf")]
	[Tooltip("Радиус в котором мы ищем позицию для побега или врага для телепортации к нему.")]
	public int Range;

	[ShowIf("IsMoveSelf")]
	[Tooltip("Если галочка стоит, то телепортироваться будет только к видимым врагам или к видимым клеткам.")]
	public bool UseLos;

	[ShowIf("IsMoveSelf")]
	[Tooltip("Если галочка стоит, то пытаемся телепортироваться как можно дальше от всех врагов. В противном случае вымираем кого-то из врагов и телепортируемся рядом с ним.")]
	public bool Escape;

	[ShowIf("IsMoveSelf")]
	[HideIf("Escape")]
	[Tooltip("Выбираем наиболее удаленного от кастера врага, но с учетом приоритета и дальности. В противном случае - ближайшего.")]
	public bool SearchFurthestEnemy;

	[ShowIf("IsMoveSelf")]
	[HideIf("Escape")]
	[Tooltip("Выбрав цель пытаемся телепортироваться рядом с ней, но как можно дальше от стартовой точки. В противном случае - как можно ближе.")]
	public bool TryJumpOverEnemy;

	[ShowIf("IsMoveSelf")]
	[HideIf("Escape")]
	[Tooltip("По возможности пытаемся телепортироваться к врагам, подпадающим под эти условия")]
	public PropertyCalculator[] EnemyPriorityConditions;

	[Space(4f)]
	public GameObject PortalFromPrefab;

	public GameObject PortalToPrefab;

	public string PortalBone = "";

	public GameObject CasterDisappearFx;

	public GameObject CasterAppearFx;

	public GameObject SideDisappearFx;

	public GameObject SideAppearFx;

	[SerializeField]
	private BlueprintProjectileReference m_CasterDisappearProjectile;

	[SerializeField]
	private BlueprintProjectileReference m_CasterAppearProjectile;

	[SerializeField]
	private BlueprintProjectileReference m_SideDisappearProjectile;

	[SerializeField]
	private BlueprintProjectileReference m_SideAppearProjectile;

	public bool LookAtRandomDirection;

	public ActionList ActionsOnCasterAfter;

	[ShowIf("IsMoveTarget")]
	public ActionList ActionsOnTargetAfter;

	private bool IsMoveSelf => TeleportationType == TeleportationType.MoveSelf;

	private bool IsMoveTarget => TeleportationType == TeleportationType.MoveTarget;

	public BlueprintProjectile CasterDisappearProjectile => m_CasterDisappearProjectile?.Get();

	public BlueprintProjectile CasterAppearProjectile => m_CasterAppearProjectile?.Get();

	public BlueprintProjectile SideDisappearProjectile => m_SideDisappearProjectile?.Get();

	public BlueprintProjectile SideAppearProjectile => m_SideAppearProjectile?.Get();

	public override bool IsEngageUnit => true;

	public override IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TargetWrapper target)
	{
		yield break;
	}

	private static IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, TeleportSettings settings, BaseUnitEntity caster, TargetWrapper target)
	{
		List<BaseUnitEntity> targets = settings.Targets;
		Vector3 point = target.Point;
		Span<Vector3> pointsArray = stackalloc Vector3[targets.Count];
		Span<float> radiusArray = stackalloc float[targets.Count];
		for (int i = 0; i < targets.Count; i++)
		{
			pointsArray[i] = (Vector3)ObstacleAnalyzer.GetNearestNode(targets[i].Position - caster.Position + point).node.position;
			radiusArray[i] = targets[i].Corpulence;
		}
		if (settings.RelaxPoints)
		{
			FreePlaceSelector.RelaxPoints(pointsArray, radiusArray, targets.Count);
		}
		Vector3 vector = pointsArray[0];
		GameObject gameObject = FxHelper.SpawnFxOnEntity(settings.PortalFromPrefab, caster.View);
		GameObject gameObject2 = FxHelper.SpawnFxOnEntity(settings.PortalToPrefab, caster.View);
		if (gameObject2 != null)
		{
			gameObject2.transform.position = vector;
		}
		Vector3 value = ObjectExtensions.Or(ObjectExtensions.Or(gameObject, null)?.transform.FindChildRecursive(settings.PortalBone), null)?.transform.position ?? caster.Position;
		Vector3 value2 = ObjectExtensions.Or(ObjectExtensions.Or(gameObject2, null)?.transform.FindChildRecursive(settings.PortalBone), null)?.transform.position ?? vector;
		List<IEnumerator> teleportationRoutines = new List<IEnumerator>();
		for (int j = 0; j < targets.Count; j++)
		{
			BaseUnitEntity baseUnitEntity = targets[j];
			Vector3 targetPosition = pointsArray[j];
			baseUnitEntity.Wake(10f);
			Vector3? intermediateFromPosition = ((gameObject != null) ? new Vector3?(value) : null);
			Vector3? intermediateToPosition = ((gameObject2 != null) ? new Vector3?(value2) : null);
			IEnumerator item = CreateTeleportationRoutine(context, settings, baseUnitEntity, target, targetPosition, intermediateFromPosition, intermediateToPosition, j == 0);
			teleportationRoutines.Add(item);
		}
		TimeSpan startTime = Game.Instance.Controllers.TimeController.GameTime;
		while (teleportationRoutines.Count > 0 && Game.Instance.Controllers.TimeController.GameTime - startTime < MaxTeleportationDuration)
		{
			for (int k = 0; k < teleportationRoutines.Count; k++)
			{
				if (!teleportationRoutines[k].MoveNext())
				{
					teleportationRoutines.RemoveAt(k);
					k--;
				}
			}
			yield return null;
		}
		yield return new AbilityDeliveryTarget(target);
	}

	private static IEnumerator CreateTeleportationRoutine(AbilityExecutionContext context, TeleportSettings settings, BaseUnitEntity unit, TargetWrapper spellTarget, Vector3 targetPosition, Vector3? intermediateFromPosition, Vector3? intermediateToPosition, bool isCaster)
	{
		GameObject prefab = (isCaster ? settings.CasterDisappearFx : settings.SideDisappearFx);
		GameObject appearFx = (isCaster ? settings.CasterAppearFx : settings.SideAppearFx);
		BlueprintProjectile disappearProjectile = (isCaster ? settings.CasterDisappearProjectile : settings.SideDisappearProjectile);
		BlueprintProjectile appearProjectile = (isCaster ? settings.CasterAppearProjectile : settings.SideAppearProjectile);
		BlueprintProjectile teleportationProjectile = (isCaster ? settings.CasterTeleportationProjectile : settings.SideTeleportationProjectile);
		float appearDuration = (isCaster ? settings.CasterAppearDuration : settings.SideAppearDuration);
		float disappearDuration = (isCaster ? settings.CasterDisappearDuration : settings.SideDisappearDuration);
		appearDuration = Math.Max(appearDuration, 0.3f);
		unit.View.StopMoving();
		FxHelper.SpawnFxOnEntity(prefab, unit.View);
		if (disappearDuration > 0.01f)
		{
			TimeSpan startTime = Game.Instance.Controllers.TimeController.GameTime;
			while (Game.Instance.Controllers.TimeController.GameTime - startTime < disappearDuration.Seconds())
			{
				yield return null;
			}
		}
		if (disappearProjectile != null && intermediateFromPosition.HasValue)
		{
			IEnumerator projectileRoutine = CreateProjectileRoutine(context, disappearProjectile, unit, unit.Position, intermediateFromPosition.Value);
			while (projectileRoutine.MoveNext())
			{
				yield return null;
			}
		}
		if (teleportationProjectile != null)
		{
			Vector3 targetPosition2 = intermediateToPosition ?? targetPosition;
			IEnumerator projectileRoutine = CreateProjectileRoutine(context, teleportationProjectile, unit, intermediateFromPosition, targetPosition2);
			while (projectileRoutine.MoveNext())
			{
				yield return null;
			}
		}
		unit.View.MovementAgent.Blocker.Unblock();
		unit.Position = targetPosition;
		unit.View.MovementAgent.Blocker.BlockAt(unit.Position);
		unit.ForceTurnTo(settings.LookAtPoint);
		if (appearProjectile != null && intermediateToPosition.HasValue)
		{
			IEnumerator projectileRoutine = CreateProjectileRoutine(context, appearProjectile, unit, intermediateToPosition, targetPosition);
			while (projectileRoutine.MoveNext())
			{
				yield return null;
			}
		}
		else
		{
			yield return null;
		}
		FxHelper.SpawnFxOnEntity(appearFx, unit.View);
		if (appearDuration > 0.01f)
		{
			TimeSpan startTime = Game.Instance.Controllers.TimeController.GameTime;
			while (Game.Instance.Controllers.TimeController.GameTime - startTime < appearDuration.Seconds())
			{
				yield return null;
			}
		}
	}

	private static IEnumerator CreateProjectileRoutine(AbilityExecutionContext context, BlueprintProjectile blueprint, BaseUnitEntity unit, Vector3? sourcePosition, Vector3 targetPosition)
	{
		Projectile projectile = new ProjectileLauncher(blueprint, unit, targetPosition).Ability(context.Ability).LaunchPosition(sourcePosition).Launch();
		float distance = projectile.Distance(unit.Position, targetPosition);
		while (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
		{
			yield return null;
		}
	}

	public override void Cleanup(AbilityExecutionContext context)
	{
	}

	protected virtual Vector3 GetLookAtPoint(BaseUnitEntity caster, Vector3 targetPos)
	{
		if (Game.Instance.Controllers.TurnController.TurnBasedModeActive)
		{
			targetPos = (Vector3)ObstacleAnalyzer.GetNearestNode(targetPos).node.position;
		}
		if (LookAtRandomDirection)
		{
			return targetPos + Quaternion.AngleAxis(45 * caster.Random.Range(0, 8), Vector3.up) * Vector3.right;
		}
		return targetPos + caster.View.ViewTransform.forward;
	}

	private int DistanceToClosestEnemyInCells(GridNodeBase checkNode, IntRect rect, IEnumerable<BaseUnitEntity> enemies)
	{
		return enemies.Where((BaseUnitEntity e) => e.IsConscious).Min((BaseUnitEntity e) => e.DistanceToInCells(checkNode.Vector3Position(), rect));
	}

	private bool CanMoveByVector(MechanicEntity caster, Vector2Int movement)
	{
		foreach (GridNodeBase occupiedNode in caster.GetOccupiedNodes())
		{
			int xCoordinateInGrid = occupiedNode.XCoordinateInGrid;
			int zCoordinateInGrid = occupiedNode.ZCoordinateInGrid;
			GridGraph gridGraph = (GridGraph)occupiedNode.Graph;
			GridNodeBase node = gridGraph.GetNode(xCoordinateInGrid + movement.x, zCoordinateInGrid + movement.y);
			if (!UsableForLanding(node))
			{
				return false;
			}
			bool UsableForLanding(GridNodeBase landingNode)
			{
				if (landingNode.Walkable)
				{
					if (node.TryGetFirstUnit(out var unit) && unit.IsConscious)
					{
						return unit == caster;
					}
					return true;
				}
				return false;
			}
		}
		return true;
	}

	private bool CanCoverNode(BaseUnitEntity caster, Vector2Int nodeToCover, out Vector2Int nodeToLand)
	{
		foreach (GridNodeBase occupiedNode in caster.GetOccupiedNodes())
		{
			int xCoordinateInGrid = ((GridNodeBase)caster.CurrentNode.node).XCoordinateInGrid;
			int zCoordinateInGrid = ((GridNodeBase)caster.CurrentNode.node).ZCoordinateInGrid;
			if (CanMoveByVector(caster, nodeToCover - occupiedNode.CoordinatesInGrid))
			{
				nodeToLand = nodeToCover - new Vector2Int(xCoordinateInGrid, zCoordinateInGrid) + occupiedNode.CoordinatesInGrid;
				return true;
			}
		}
		nodeToLand = new Vector2Int(caster.CurrentNode.node.position.x, caster.CurrentNode.node.position.z);
		return false;
	}

	private bool TryFindLandingNodeAroundUnit(BaseUnitEntity caster, BaseUnitEntity targetUnit, out Vector2Int landingNode)
	{
		IEnumerable<GridNodeBase> source = from node in GridAreaHelper.GetNodesSpiralAround((GridNodeBase)targetUnit.CurrentNode.node, targetUnit.SizeRect, 1)
			where node.Walkable
			select node;
		source = (TryJumpOverEnemy ? source.OrderByDescending((GridNodeBase node) => caster.DistanceTo(node.Vector3Position())) : source.OrderBy((GridNodeBase node) => caster.DistanceTo(node.Vector3Position())));
		foreach (GridNodeBase item in source)
		{
			if (CanCoverNode(caster, new Vector2Int(item.XCoordinateInGrid, item.ZCoordinateInGrid), out landingNode))
			{
				return true;
			}
		}
		landingNode = new Vector2Int(caster.CurrentNode.node.position.x, caster.CurrentNode.node.position.z);
		return false;
	}

	private bool TryFindEnemyToLandAround(BaseUnitEntity caster, IEnumerable<BaseUnitEntity> enemies, out BaseUnitEntity validEnemy, out Vector2Int landingNode)
	{
		enemies = ((!SearchFurthestEnemy) ? enemies.OrderByDescending((BaseUnitEntity enemy) => caster.DistanceToInCells(enemy)) : enemies.OrderBy((BaseUnitEntity enemy) => caster.DistanceToInCells(enemy)));
		foreach (BaseUnitEntity enemy in enemies)
		{
			if (TryFindLandingNodeAroundUnit(caster, enemy, out landingNode))
			{
				validEnemy = enemy;
				return true;
			}
		}
		validEnemy = null;
		landingNode = new Vector2Int(caster.CurrentNode.node.position.x, caster.CurrentNode.node.position.z);
		return false;
	}

	public bool IsTargetRestrictionPassed(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		string unavailabilityReason;
		return IsValid(ability, target, out unavailabilityReason);
	}

	public string GetAbilityTargetRestrictionUIText(AbilityData ability, TargetWrapper target, Vector3 casterPosition)
	{
		IsValid(ability, target, out var unavailabilityReason);
		return unavailabilityReason;
	}

	private bool IsValid(AbilityData ability, TargetWrapper target, out string unavailabilityReason)
	{
		unavailabilityReason = null;
		switch (TeleportationType)
		{
		case TeleportationType.MoveSelf:
			return true;
		case TeleportationType.MoveTarget:
		{
			if (!target.HasEntity)
			{
				unavailabilityReason = ConfigRoot.Instance.LocalizedTexts.Reasons.TargetIsInvalid;
				return false;
			}
			if (!TryFindLandingNodeAroundUnit(target.Entity as BaseUnitEntity, ability.Caster as BaseUnitEntity, out var _))
			{
				unavailabilityReason = ConfigRoot.Instance.LocalizedTexts.Reasons.NotEnoughSpace;
				return false;
			}
			return true;
		}
		default:
			PFLog.Default.Error("unknown teleportation type");
			return true;
		}
	}
}
