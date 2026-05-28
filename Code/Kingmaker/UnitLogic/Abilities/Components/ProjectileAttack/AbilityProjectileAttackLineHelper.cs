using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Root.Fx;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Particles.Blueprints;
using Owlcat.Fmw.Blueprints;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public static class AbilityProjectileAttackLineHelper
{
	private struct AttackResultData
	{
		[CanBeNull]
		public RulePerformAttack Rule;

		[CanBeNull]
		public MechanicEntity TargetCoverEntity;
	}

	private static float s_MissTargetDistance = 30f;

	private const int VerticalDeviationLimit = 1;

	public static IEnumerator<AbilityDeliveryTarget> DeliverLine(AbilityExecutionContext context, BlueprintProjectile projectileBlueprint, AbilityProjectileAttackLine attackLine)
	{
		TargetWrapper targetWrapper = null;
		while (true)
		{
			GridNodeBase fromNode = attackLine.FromNode;
			AbilityProjectileAttackLine.HitData[] hits = attackLine.Hits;
			if (hits.Length != 0 && (hits[0].Entity?.IsDead ?? false))
			{
				attackLine.Hits = attackLine.CalculateHits().Hits.ToArray();
				hits = attackLine.Hits;
			}
			ReadonlyList<GridNodeBase> nodes = attackLine.Nodes;
			if (nodes.Count < 1)
			{
				PFLog.Default.ErrorWithReport($"Can't make attack {attackLine.Index}: projectile path is empty");
				break;
			}
			Vector3 start = fromNode.Vector3Position();
			Vector3 end = nodes[nodes.Count - 1].Vector3Position();
			Vector3 position = context.Caster.Position;
			bool isCoverHit = hits.Any((AbilityProjectileAttackLine.HitData h) => h.RollPerformAttackRule.ResultIsCoverHit);
			TargetWrapper targetWrapper2 = ((targetWrapper != null) ? targetWrapper : new TargetWrapper(position, null, context.Caster));
			int value = context.Ability.RangeCells;
			if (hits.Length != 0)
			{
				AbilityProjectileAttackLine.HitData[] array = hits;
				for (int i = 0; i < array.Length; i++)
				{
					AbilityProjectileAttackLine.HitData hitData = array[i];
					if (hitData.IsRedirecting)
					{
						isCoverHit = true;
						value = WarhammerGeometryUtils.DistanceToInCells(targetWrapper2.Point, default(IntRect), hitData.Entity.Center, default(IntRect));
						break;
					}
				}
			}
			TargetWrapper projectileTarget = GetProjectileTarget(context, attackLine, hits, (attackLine.Index < context.ProjectileHitPositions.Count) ? new Vector3?(context.ProjectileHitPositions[attackLine.Index]) : null);
			Vector3 projectileMisdirectionOffset = GetProjectileMisdirectionOffset(position, projectileTarget.Point, 0.15f);
			Projectile projectile = new ProjectileLauncher(projectileBlueprint, targetWrapper2, projectileTarget).Ability(context.Ability).MaxRangeCells(value).Index(attackLine.Index)
				.MisdirectionOffset(projectileMisdirectionOffset)
				.IsCoverHit(isCoverHit)
				.Launch();
			attackLine.Projectile = projectile;
			Debug.DrawLine(start, end, Color.yellow);
			yield return null;
			AbilityProjectileAttackLine.HitData[] array2 = hits;
			foreach (AbilityProjectileAttackLine.HitData hitData2 in array2)
			{
				foreach (AbilityDeliveryTarget item in HandleHit(attackLine, hitData2))
				{
					yield return item;
				}
			}
			if (hits.Length == 0 || !hits.Last().IsRedirecting)
			{
				break;
			}
			MechanicEntity deflector = hits.Last().Entity;
			if (deflector == null)
			{
				break;
			}
			List<MechanicEntity> list = (from p in Game.Instance.EntityPools.AllUnits
				where !p.Features.IsUntargetable && !p.LifeState.IsDead && p.IsInCombat && p.Health.HitPointsLeft > 0
				where p.Facts.GetComponents<DeflectionTarget>().Any((DeflectionTarget c) => c.Caster == deflector)
				select p).Cast<MechanicEntity>().ToList();
			list.Remove(hits.Last().Entity);
			list.RemoveAll((MechanicEntity p) => !p.IsEnemy(deflector));
			list.RemoveAll((MechanicEntity p) => !deflector.HasLOS(p));
			list.RemoveAll((MechanicEntity p) => deflector.DistanceToInCells(p) > context.Ability.RangeCells);
			if (!list.Empty())
			{
				MechanicEntity target = list.MinBy((MechanicEntity p) => deflector.DistanceToInCells(p));
				(List<GridNodeBase>, GridNodeBase, GridNodeBase) tuple = AbilityProjectileAttack.CollectNodes((GridNodeBase)deflector.CurrentNode.node, target, context.Ability.RangeCells);
				attackLine = new AbilityProjectileAttackLine(attackLine.ProjectileAttack, attackLine.Index, tuple.Item2, tuple.Item3, tuple.Item1, attackLine.WeaponAttackDamageDisabled, disableDodgeForAlly: true);
				targetWrapper = new TargetWrapper(deflector.Center);
				continue;
			}
			break;
		}
	}

	private static TargetWrapper GetProjectileTarget(AbilityExecutionContext context, AbilityProjectileAttackLine attackLine, AbilityProjectileAttackLine.HitData[] hits, Vector3? precalculatedPosition = null)
	{
		GridNodeBase node = attackLine.Nodes.LastOrDefault((GridNodeBase x) => IsNodeAffected(null, attackLine.FromNode, x, attackLine.StepHeight)) ?? attackLine.Nodes.Last();
		AbilityProjectileAttackLine.HitData lastHit = hits.LastItem();
		AbilityProjectileAttackLine.HitData hitData = hits.LastItem((AbilityProjectileAttackLine.HitData i) => i.Entity is UnitEntity && i.RollPerformAttackRule.ResultIsHit);
		Vector3 vector;
		if (hitData.Empty)
		{
			if (!lastHit.Empty)
			{
				RulePerformAttackRoll rollPerformAttackRule = lastHit.RollPerformAttackRule;
				if (rollPerformAttackRule != null && !rollPerformAttackRule.IsOverpenetration && rollPerformAttackRule.ResultIsHit)
				{
					vector = lastHit.Node.Vector3Position();
					vector.y = node.Vector3Position().y + 1f;
					return vector;
				}
			}
			Vector3 eyePosition = context.Caster.EyePosition;
			AbilityProjectileAttackLine.HitData hitData2 = hits.LastItem((AbilityProjectileAttackLine.HitData i) => i.Entity is UnitEntity);
			if (hitData2.Empty)
			{
				vector = node.Vector3Position();
				vector.y = attackLine.ToNode.Vector3Position().y + 1f;
				eyePosition.y = vector.y;
				return eyePosition + (vector - eyePosition).normalized * s_MissTargetDistance;
			}
			if (TryGetTargetPointByRandomLocator(hitData.Entity, context, lastHit, out vector))
			{
				return vector;
			}
			vector = node.Vector3Position();
			vector.y = hitData2.Node.Vector3Position().y + 1f;
			eyePosition.y = vector.y;
			return eyePosition + (vector - eyePosition).normalized * s_MissTargetDistance;
		}
		if (precalculatedPosition.HasValue)
		{
			return precalculatedPosition;
		}
		if (TryGetTargetPointByRandomLocator(hitData.Entity, context, lastHit, out vector))
		{
			return vector;
		}
		vector = lastHit.Node.Vector3Position();
		vector.y = node.Vector3Position().y + 1f;
		return vector;
	}

	private static bool TryGetTargetPointByRandomLocator(MechanicEntity unit, AbilityExecutionContext context, AbilityProjectileAttackLine.HitData lastHit, out Vector3 result)
	{
		result = default(Vector3);
		if (!(unit is UnitEntity unitEntity))
		{
			return false;
		}
		BpRef<BlueprintFxLocatorGroup> bpRef = lastHit.RollPerformAttackRule.ResultHitLocation.FxLocatorGroups?.ElementAtOrDefault(0);
		BlueprintFxLocatorGroup group = (((object)bpRef != null) ? ((BlueprintFxLocatorGroup?)bpRef) : FxRoot.Instance.LocatorGroupDefaultHit);
		FxBone fxBone = ObjectExtensions.Or(unitEntity.View.ParticlesSnapMap, null)?.GetLocators(group).Random(PFStatefulRandom.UnitLogic.Abilities);
		if (fxBone == null)
		{
			return false;
		}
		result = fxBone.Transform.position;
		return true;
	}

	private static IEnumerable<AbilityDeliveryTarget> HandleHit(AbilityProjectileAttackLine attackLine, AbilityProjectileAttackLine.HitData hitData)
	{
		GridNodeBase node = hitData.Node;
		AbilityExecutionContext context = attackLine.Context;
		Projectile projectile = attackLine.Projectile;
		GridNodeBase fromNode = attackLine.FromNode;
		Vector3 casterPosition = fromNode.Vector3Position();
		ReadonlyList<GridNodeBase> nodes = attackLine.Nodes;
		Vector3 targetPosition = nodes[nodes.Count - 1].Vector3Position();
		float distance = projectile.Distance(node.Vector3Position(), context.Caster.Position);
		while (!projectile.IsEnoughTimePassedToTraverseDistance(distance))
		{
			Debug.DrawLine(casterPosition, targetPosition, Color.yellow);
			yield return null;
		}
		Debug.DrawLine(casterPosition, targetPosition, Color.yellow);
		Debug.DrawLine(node.Vector3Position(), node.Vector3Position() + Vector3.up * 3f, Color.yellow);
		MechanicEntity currentTarget = hitData.Entity;
		if (currentTarget != null)
		{
			AttackResultData attack = MakeAttack(context, attackLine, hitData, projectile);
			if (attack.TargetCoverEntity != null)
			{
				Debug.DrawLine(node.Vector3Position(), attack.TargetCoverEntity.Position + Vector3.up * 3f, Color.red);
				yield return new AbilityDeliveryTarget(attack.TargetCoverEntity)
				{
					AttackRule = attack.Rule,
					Projectile = projectile
				};
			}
			Debug.DrawLine(node.Vector3Position(), currentTarget.Position + Vector3.up * 3f, Color.red);
			yield return new AbilityDeliveryTarget(currentTarget)
			{
				AttackRule = attack.Rule,
				Projectile = projectile
			};
		}
	}

	private static AttackResultData MakeAttack(AbilityExecutionContext context, AbilityProjectileAttackLine attackLine, AbilityProjectileAttackLine.HitData hitData, Projectile projectile)
	{
		if (attackLine.ProjectileAttack.AttacksDisabled || hitData.Entity == null)
		{
			return default(AttackResultData);
		}
		RulePerformAttack rulePerformAttack = new RulePerformAttack(context.Caster, hitData.Entity, context.Ability, attackLine.Index, attackLine.WeaponAttackDamageDisabled, attackLine.DodgeForAllyDisabled, hitData.RollPerformAttackRule)
		{
			FromOverpenetration = hitData.FromOverpenetration,
			Projectile = projectile,
			Reason = context
		};
		rulePerformAttack.RollPerformAttackRule.DangerArea.UnionWith(attackLine.Nodes);
		context.TriggerRule(rulePerformAttack);
		MechanicEntity resultCoverEntity = rulePerformAttack.RollPerformAttackRule.ResultCoverEntity;
		AttackResultData result = default(AttackResultData);
		result.Rule = rulePerformAttack;
		result.TargetCoverEntity = resultCoverEntity;
		return result;
	}

	public static bool IsNodeAffected(IAbilityDataProviderForPattern ability, GridNodeBase fromNode, GridNodeBase targetNode, float stepHeight, bool ignoreLos = false, bool ignoreLevelDifference = false)
	{
		if (!ignoreLos)
		{
			using (ProfileScope.New("HasLos"))
			{
				if (ability != null)
				{
					if (!ability.HasLosCached(fromNode, targetNode))
					{
						return false;
					}
				}
				else if (!LosCalculations.HasLos(fromNode, default(IntRect), targetNode, default(IntRect)))
				{
					return false;
				}
			}
		}
		if (!ignoreLevelDifference)
		{
			int num = Mathf.Max(Mathf.Abs(fromNode.XCoordinateInGrid - targetNode.XCoordinateInGrid), Mathf.Abs(fromNode.ZCoordinateInGrid - targetNode.ZCoordinateInGrid));
			float num2 = fromNode.Vector3Position().y + (float)num * stepHeight;
			if (Mathf.Abs(targetNode.Vector3Position().y - num2) <= 1f)
			{
				return true;
			}
			foreach (DestructibleEntity destructibleEntity in Game.Instance.EntityPools.DestructibleEntities)
			{
				if (destructibleEntity.CanBeAttackedDirectly && destructibleEntity.GetOccupiedNodes().Contains(targetNode) && num2 >= destructibleEntity.Position.y - 1f && num2 <= targetNode.Vector3Position().y + 1f)
				{
					return true;
				}
			}
			return false;
		}
		return true;
	}

	public static float GetStepHeightBetweenCells(GridNodeBase fromNode, GridNodeBase toNode)
	{
		int num = Mathf.Max(Mathf.Abs(fromNode.XCoordinateInGrid - toNode.XCoordinateInGrid), Mathf.Abs(fromNode.ZCoordinateInGrid - toNode.ZCoordinateInGrid));
		return (toNode.Vector3Position().y - fromNode.Vector3Position().y) / (float)num;
	}

	private static Vector3 GetProjectileMisdirectionOffset(Vector3 from, Vector3 to, float radius)
	{
		Vector3 normalized = (from - to).normalized;
		Vector3 vector = new Vector3(normalized.z, 0f, normalized.x);
		Vector3 up = Vector3.up;
		return PFStatefulRandom.UnitLogic.Abilities.Range(0f - radius, radius) * vector + PFStatefulRandom.UnitLogic.Abilities.Range(0f - radius, radius) * up;
	}

	private static OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, bool coveredTargetsOnly, int? builtInHaloSize, int? haloSize, bool ignoreLos = false, bool ignoreLevelDifference = false, int patternAngle = 30)
	{
		patternAngle = ((patternAngle > 0) ? patternAngle : 30);
		GridNodeBase gridNodeBase = casterNode;
		if (ability.Caster.SizeRect != default(IntRect))
		{
			int num = int.MaxValue;
			foreach (GridNodeBase occupiedNode in ability.Caster.GetOccupiedNodes(casterNode))
			{
				int num2 = WarhammerGeometryUtils.DistanceToInCells(casterNode.CoordinatesInGrid, default(IntRect), occupiedNode.CoordinatesInGrid, default(IntRect));
				if (num2 < num && LosCalculations.HasLos(occupiedNode, default(IntRect), targetNode, default(IntRect)))
				{
					gridNodeBase = occupiedNode;
					num = num2;
				}
			}
		}
		Vector2 normalized = (targetNode.Vector3Position() - gridNodeBase.Vector3Position()).To2D().normalized;
		PatternGridData patternGridData = GridPatterns.ConstructPattern(PatternType.Cone, ability.RangeCells, patternAngle, normalized, ability.Caster.Size);
		if (builtInHaloSize.HasValue)
		{
			PatternGridData patternGridData2 = patternGridData;
			patternGridData = patternGridData2.BuildHalo(builtInHaloSize.Value, PatternGridData.HaloMode.IncludeOriginalPattern, gridNodeBase.CoordinatesInGrid, normalized, preventBlowback: true, disposable: true);
			patternGridData2.Dispose();
		}
		if (haloSize.HasValue)
		{
			PatternGridData patternGridData3 = patternGridData;
			patternGridData = patternGridData3.BuildHalo(haloSize.Value, PatternGridData.HaloMode.ExcludeOriginalPattern, gridNodeBase.CoordinatesInGrid, normalized, preventBlowback: true, disposable: true);
			patternGridData3.Dispose();
		}
		GridGraph graph = (GridGraph)casterNode.Graph;
		Vector2Int vector2Int = new Linecast.Ray2NodeOffsets(Vector2Int.zero, normalized).Take(2).Last();
		Vector2Int offset = gridNodeBase.CoordinatesInGrid + vector2Int;
		PatternGridData pattern = patternGridData.Move(offset);
		NodeList nodeList = new NodeList(graph, in pattern);
		float stepHeightBetweenCells = GetStepHeightBetweenCells(gridNodeBase, targetNode);
		Dictionary<GridNodeBase, PatternCellDataAccumulator> dictionary = new Dictionary<GridNodeBase, PatternCellDataAccumulator>();
		TempList.Get<float>().Capacity = ability.BurstAttacksCount;
		foreach (GridNodeBase item in nodeList)
		{
			if (item == gridNodeBase)
			{
				continue;
			}
			MechanicEntity mechanicEntity = (MechanicEntity)(((object)item.GetFirstUnit()) ?? ((object)item.GetDestructibleEntities().FirstOrDefault()));
			if (coveredTargetsOnly && mechanicEntity == null)
			{
				continue;
			}
			float defenceProbability = 1f;
			float coverProbability = 0f;
			float evasionProbability = 0f;
			float num3 = 0f;
			BaseUnitEntity baseUnitEntity = mechanicEntity as BaseUnitEntity;
			if (mechanicEntity == null)
			{
				defenceProbability = 0f;
				goto IL_02a6;
			}
			using (ProfileScope.New("IsNodeAffected"))
			{
				if (!IsNodeAffected(ability, gridNodeBase, item, stepHeightBetweenCells, ignoreLos, ignoreLevelDifference) || ability.Caster.GetOccupiedNodes(casterNode).Contains(item))
				{
					continue;
				}
				goto IL_02a6;
			}
			IL_02a6:
			Vector3 vector = gridNodeBase.Vector3Position() - item.Vector3Position();
			if (mechanicEntity != null)
			{
				RuleCalculateHitChances ruleCalculateHitChances = new RuleCalculateHitChances(ability.Caster, mechanicEntity, ability.Data, 0);
				Rulebook.Trigger(ruleCalculateHitChances);
				num3 = (float)ruleCalculateHitChances.ResultHitChance / 100f;
			}
			if (baseUnitEntity != null && vector.magnitude > 0.1f)
			{
				int direction = GraphHelper.GuessDirection(vector.normalized);
				LosDescription cellCoverStatus = LosCalculations.GetCellCoverStatus(item, direction);
				coverProbability = cellCoverStatus.CoverType switch
				{
					LosCalculations.CoverType.Obstacle => 0f, 
					LosCalculations.CoverType.LosBlocker => 1f, 
					_ => (float)baseUnitEntity.BodyParts.Where((BlueprintBodyPart i) => i.ReplaceableByCover).Sum((BlueprintBodyPart i) => i.HitChance) / 100f, 
				};
				defenceProbability = (baseUnitEntity.IsDead ? 1f : ability.CalculateDefenceChanceCached((UnitEntity)baseUnitEntity, cellCoverStatus));
			}
			dictionary[item] = new PatternCellDataAccumulator(new float[1] { num3 }, defenceProbability, coverProbability, evasionProbability, mainCell: true);
		}
		return new OrientedPatternData(dictionary, gridNodeBase);
	}

	public static OrientedPatternData GetOrientedPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, bool coveredTargetsOnly = false, int? builtInHaloSize = null, bool ignoreLos = false, bool ignoreLevelDifference = false, int patternAngle = 30)
	{
		return GetOrientedPattern(ability, casterNode, targetNode, coveredTargetsOnly, builtInHaloSize, null, ignoreLos, ignoreLevelDifference, patternAngle);
	}

	public static OrientedPatternData GetOrientedHaloPattern(IAbilityDataProviderForPattern ability, GridNodeBase casterNode, GridNodeBase targetNode, bool coveredTargetsOnly = false, int? builtInHaloSize = null, int? haloSize = null)
	{
		return GetOrientedPattern(ability, casterNode, targetNode, coveredTargetsOnly, builtInHaloSize, haloSize);
	}

	[CanBeNull]
	public static MechanicEntity GetTargetByNode(GridNodeBase node)
	{
		BaseUnitEntity firstUnit = node.GetFirstUnit();
		if (firstUnit != null)
		{
			return firstUnit;
		}
		DestructibleEntity destructibleEntity = null;
		foreach (DestructibleEntity destructibleEntity2 in node.GetDestructibleEntities())
		{
			if (!destructibleEntity2.Health.IsFullyDamaged)
			{
				return destructibleEntity2;
			}
			if (destructibleEntity == null)
			{
				destructibleEntity = destructibleEntity2;
			}
		}
		return destructibleEntity;
	}

	public static IEnumerable<(MechanicEntity Entity, LosDescription Los, GridNodeBase Node)> EnumerateTargets([NotNull] AbilityExecutionContext context, [CanBeNull] MechanicEntity priorityTarget, [NotNull] IEnumerable<GridNodeBase> nodes, [NotNull] GridNodeBase fromNode, [NotNull] GridNodeBase toNode, bool ignoreLos = false, bool ignoreLevelDifference = false)
	{
		List<MechanicEntity> targets = TempList.Get<MechanicEntity>();
		if (priorityTarget is ThinCoverEntity)
		{
			targets.Add(priorityTarget);
			yield return (Entity: priorityTarget, Los: new LosDescription(LosCalculations.CoverType.Obstacle), Node: priorityTarget.CurrentUnwalkableNode);
		}
		foreach (GridNodeBase node in nodes)
		{
			float stepHeightBetweenCells = GetStepHeightBetweenCells(fromNode, toNode);
			if (IsNodeAffected(null, fromNode, node, stepHeightBetweenCells, ignoreLos, ignoreLevelDifference))
			{
				MechanicEntity targetByNode = GetTargetByNode(node);
				if (targetByNode != null && targetByNode != context.Caster && context.Ability.IsValidTargetForAttack(targetByNode) && !targets.Contains(targetByNode))
				{
					MechanicEntity caster = context.Caster;
					LosDescription warhammerLos = LosCalculations.GetWarhammerLos(fromNode, caster.SizeRect, node, targetByNode.SizeRect);
					targets.Add(targetByNode);
					yield return (Entity: targetByNode, Los: warhammerLos, Node: node);
				}
			}
		}
	}
}
