using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.QA;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.RuleBurst;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public class AbilityProjectileAttack : IEnumerator<AbilityDeliveryTarget>, IEnumerator, IDisposable
{
	[NotNull]
	private readonly IEnumerator<AbilityDeliveryTarget> m_Process;

	[NotNull]
	[ItemCanBeNull]
	private readonly AbilityProjectileAttackLine[] m_Attacks;

	[NotNull]
	public readonly AbilityExecutionContext Context;

	[CanBeNull]
	public readonly MechanicEntity PriorityTarget;

	[CanBeNull]
	public readonly List<MechanicEntity> UnitsInPattern;

	public AbilityProjectileAttackLine[] Attacks => m_Attacks;

	public bool IsControlledScatter { get; }

	public bool AttacksDisabled { get; private set; }

	public bool OverpenetrationDisabled { get; private set; }

	public AttackHitPolicyType AttackHitPolicy { get; private set; }

	[CanBeNull]
	public AbilityDeliveryTarget CurrentTarget { get; private set; }

	public bool IsFinished { get; private set; }

	public bool WeaponAttackDamageDisabled { get; private set; }

	public bool DodgeForAllyDisabled { get; private set; }

	public int Count => m_Attacks.Length;

	public AbilityProjectileAttackLine this[int index]
	{
		get
		{
			return m_Attacks[index];
		}
		set
		{
			m_Attacks[index] = value;
		}
	}

	AbilityDeliveryTarget IEnumerator<AbilityDeliveryTarget>.Current => CurrentTarget;

	object IEnumerator.Current => CurrentTarget;

	public AbilityProjectileAttack(AbilityExecutionContext context, int shotsCount, bool controlledScatter, List<MechanicEntity> unitsInPattern)
	{
		Context = context;
		IsControlledScatter = controlledScatter;
		UnitsInPattern = unitsInPattern;
		m_Attacks = new AbilityProjectileAttackLine[shotsCount];
		m_Process = CreateProcess();
	}

	public AbilityProjectileAttack(AbilityExecutionContext context, MechanicEntity priorityTarget, int shotsCount)
		: this(context, shotsCount, controlledScatter: false, null)
	{
		PriorityTarget = priorityTarget;
	}

	public static AbilityProjectileAttack CreatePatternBurst(AbilityExecutionContext context, TargetWrapper target, int shotsCount, bool controlledScatter)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		AbilityData ability = context.Ability;
		GridNodeBase casterNode = ability.GetBestShootingPositionForDesiredPosition(target);
		GridNodeBase gridNodeBase = ((!target.HasEntity) ? target.Point.GetNearestNodeXZUnwalkable() : (target.Entity.GetOccupiedNodes().FirstOrDefault((GridNodeBase node) => LosCalculations.GetDirectLos(casterNode.Vector3Position(), node.Vector3Position())) ?? target.Point.GetNearestNodeXZUnwalkable()));
		GridNodeBase gridNodeBase2 = (ability.UseBestShootingPosition ? LosCalculations.GetBestShootingNode(casterNode, default(IntRect), gridNodeBase, default(IntRect)) : casterNode);
		List<MechanicEntity> unitsInPattern = (from x in EnumerateTargets(context, context.Ability.Blueprint.PatternSettings.GetOrientedPattern(context.Ability, gridNodeBase2, gridNodeBase, maybeCaster.Size, coveredTargetsOnly: true).Nodes, gridNodeBase2, gridNodeBase)
			select x.Entity).ToTempList();
		AbilityProjectileAttack abilityProjectileAttack = new AbilityProjectileAttack(context, shotsCount, controlledScatter, unitsInPattern);
		int value = context.Ability.RangeCells.Cells().Value;
		List<GridNodeBase>[] list = GridPatterns.CalcScatterShot(gridNodeBase2, gridNodeBase, value);
		for (int i = 0; i < abilityProjectileAttack.Count; i++)
		{
			if (!MakeSecondaryLine(i, abilityProjectileAttack, context, unitsInPattern))
			{
				List<GridNodeBase> nodes = list.Random(PFStatefulRandom.UnitLogic.Abilities);
				AbilityProjectileAttackLine line = abilityProjectileAttack.GetLine(i, gridNodeBase2, gridNodeBase, nodes);
				line.Hits = Array.Empty<AbilityProjectileAttackLine.HitData>();
				abilityProjectileAttack[i] = line;
			}
		}
		abilityProjectileAttack.RollCritEffectBodyPart();
		return abilityProjectileAttack;
	}

	public static AbilityProjectileAttack CreateBurst(AbilityExecutionContext context, TargetWrapper target, int shotsCount, bool controlledScatter)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		MechanicEntity mechanicEntity = target.Entity ?? target.NearestNode.GetFirstUnit();
		(List<GridNodeBase>, GridNodeBase, GridNodeBase) singleShotAffectedNodes = GetSingleShotAffectedNodes(context.Ability, mechanicEntity);
		List<MechanicEntity> list = (from x in EnumerateTargets(context, context.Ability.Blueprint.PatternSettings.GetOrientedPattern(context.Ability, singleShotAffectedNodes.Item2, singleShotAffectedNodes.Item3, maybeCaster.Size, coveredTargetsOnly: true).Nodes, singleShotAffectedNodes.Item2, singleShotAffectedNodes.Item3)
			select x.Entity).ToTempList();
		AbilityProjectileAttack abilityProjectileAttack = new AbilityProjectileAttack(context, shotsCount, controlledScatter, list);
		if (mechanicEntity == null)
		{
			PFLog.Default.Error("Scatter cant target point without unit!");
			return abilityProjectileAttack;
		}
		for (int num = 0; num < abilityProjectileAttack.Count; num++)
		{
			AbilityProjectileAttackLine line = abilityProjectileAttack.GetLine(num, singleShotAffectedNodes.Item2, singleShotAffectedNodes.Item3, singleShotAffectedNodes.Item1);
			(IEnumerable<AbilityProjectileAttackLine.HitData> Hits, bool HitCover) tuple = line.CalculateHits();
			IEnumerable<AbilityProjectileAttackLine.HitData> item = tuple.Hits;
			bool item2 = tuple.HitCover;
			line.Hits = item.ToArray();
			AbilityProjectileAttackLine.HitData? hitData = null;
			AbilityProjectileAttackLine.HitData[] hits = line.Hits;
			for (int i = 0; i < hits.Length; i++)
			{
				AbilityProjectileAttackLine.HitData value = hits[i];
				if (value.Entity == mechanicEntity)
				{
					hitData = value;
					break;
				}
			}
			abilityProjectileAttack[num] = line;
			int num2;
			if (hitData.HasValue)
			{
				RulePerformAttackRoll rollPerformAttackRule = hitData.GetValueOrDefault().RollPerformAttackRule;
				if (rollPerformAttackRule != null)
				{
					num2 = (rollPerformAttackRule.ResultIsHit ? 1 : 0);
					goto IL_0191;
				}
			}
			num2 = 0;
			goto IL_0191;
			IL_0191:
			if (((uint)num2 | (item2 ? 1u : 0u)) == 0)
			{
				List<MechanicEntity> list2 = list.ToTempList();
				list2.Remove(mechanicEntity);
				MakeSecondaryLine(num, abilityProjectileAttack, context, list2);
			}
		}
		abilityProjectileAttack.RollCritEffectBodyPart();
		return abilityProjectileAttack;
	}

	private static bool MakeSecondaryLine(int index, AbilityProjectileAttack attack, AbilityExecutionContext context, List<MechanicEntity> unitsInPattern)
	{
		RuleCalculateTargetInBurst ruleCalculateTargetInBurst = RuleCalculateTargetInBurst.Setup(context.Caster).WithTargets(unitsInPattern).Create();
		Rulebook.Trigger(ruleCalculateTargetInBurst);
		MechanicEntity resultTarget = ruleCalculateTargetInBurst.ResultTarget;
		if (resultTarget == null)
		{
			return false;
		}
		(List<GridNodeBase>, GridNodeBase, GridNodeBase) singleShotAffectedNodes = GetSingleShotAffectedNodes(context.Ability, resultTarget);
		AbilityProjectileAttackLine line = attack.GetLine(index, singleShotAffectedNodes.Item2, singleShotAffectedNodes.Item3, singleShotAffectedNodes.Item1);
		line.Hits = line.CalculateHits(autoHitFirst: true).Hits.ToArray();
		attack[index] = line;
		return true;
	}

	private void RollCritEffectBodyPart()
	{
		(from h in m_Attacks.SelectMany((AbilityProjectileAttackLine l) => l?.Hits ?? Array.Empty<AbilityProjectileAttackLine.HitData>()).Where(delegate(AbilityProjectileAttackLine.HitData h)
			{
				if (h.Entity != null)
				{
					RulePerformAttackRoll rollPerformAttackRule = h.RollPerformAttackRule;
					if (rollPerformAttackRule != null)
					{
						return rollPerformAttackRule.Result == AttackResult.Hit;
					}
				}
				return false;
			})
			group h by h.Entity into hs
			select hs.Random(PFStatefulRandom.Mechanics)).ForEach(delegate(AbilityProjectileAttackLine.HitData h)
		{
			h.RollPerformAttackRule.CanApplyCriticalEffect = true;
		});
	}

	private static IEnumerable<(MechanicEntity Entity, LosDescription Los, GridNodeBase Node)> EnumerateTargets(AbilityExecutionContext context, NodeList nodes, GridNodeBase fromNode, GridNodeBase toNode)
	{
		List<MechanicEntity> targets = TempList.Get<MechanicEntity>();
		foreach (GridNodeBase item in nodes)
		{
			float stepHeightBetweenCells = AbilityProjectileAttackLineHelper.GetStepHeightBetweenCells(fromNode, toNode);
			if (AbilityProjectileAttackLineHelper.IsNodeAffected(null, fromNode, item, stepHeightBetweenCells))
			{
				MechanicEntity targetByNode = GetTargetByNode(item);
				if (targetByNode != null && targetByNode != context.Caster && context.Ability.IsValidTargetForAttack(targetByNode) && !targets.Contains(targetByNode))
				{
					MechanicEntity caster = context.Caster;
					LosDescription warhammerLos = LosCalculations.GetWarhammerLos(fromNode, caster.SizeRect, item, targetByNode.SizeRect);
					targets.Add(targetByNode);
					yield return (Entity: targetByNode, Los: warhammerLos, Node: item);
				}
			}
		}
	}

	public static AbilityProjectileAttack CreateSingleTarget(AbilityExecutionContext context, MechanicEntity priorityTarget, int shotsCount)
	{
		if (context.MaybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		if (priorityTarget == null)
		{
			PFLog.Default.ErrorWithReport("PriorityTarget is missing");
			return null;
		}
		(List<GridNodeBase>, GridNodeBase, GridNodeBase) singleShotAffectedNodes = GetSingleShotAffectedNodes(context.Ability, priorityTarget);
		AbilityProjectileAttack abilityProjectileAttack = new AbilityProjectileAttack(context, priorityTarget, shotsCount);
		for (int i = 0; i < abilityProjectileAttack.Count; i++)
		{
			AbilityProjectileAttackLine line = abilityProjectileAttack.GetLine(i, singleShotAffectedNodes.Item2, singleShotAffectedNodes.Item3, singleShotAffectedNodes.Item1);
			line.Hits = line.CalculateHits().Hits.ToArray();
			abilityProjectileAttack[i] = line;
		}
		abilityProjectileAttack.RollCritEffectBodyPart();
		return abilityProjectileAttack;
	}

	public static (List<GridNodeBase> Nodes, GridNodeBase From, GridNodeBase To) GetSingleShotAffectedNodes(AbilityData ability, MechanicEntity target)
	{
		int num = ability.RangeCells.Cells().Value;
		GridNodeBase casterNode = ability.GetBestShootingPositionForDesiredPosition(target);
		GridNodeBase gridNodeBase = target.GetOccupiedNodes().FirstOrDefault((GridNodeBase node) => LosCalculations.GetDirectLos(casterNode.Vector3Position(), node.Vector3Position())) ?? target.Position.GetNearestNodeXZUnwalkable();
		GridNodeBase gridNodeBase2 = (ability.UseBestShootingPosition ? LosCalculations.GetBestShootingNode(casterNode, default(IntRect), gridNodeBase, default(IntRect)) : casterNode);
		if (gridNodeBase == gridNodeBase2)
		{
			return (Nodes: TempList.Get<GridNodeBase>(), From: gridNodeBase2, To: gridNodeBase);
		}
		if (gridNodeBase != null)
		{
			int warhammerLength = GraphHelper.GetWarhammerLength(gridNodeBase.CoordinatesInGrid - gridNodeBase2.CoordinatesInGrid);
			if (warhammerLength > num && WarhammerGeometryUtils.DistanceToInCells(casterNode.Vector3Position(), ability.Caster.SizeRect, target.Position, target.SizeRect) <= num)
			{
				num = warhammerLength;
			}
		}
		return CollectNodes(gridNodeBase2, gridNodeBase, target, num);
	}

	private static (List<GridNodeBase> Nodes, GridNodeBase From, GridNodeBase To) CollectNodes(GridNodeBase fromNode, GridNodeBase toNode, MechanicEntity target, int range)
	{
		Linecast.Ray2NodeOffsets offsets = new Linecast.Ray2NodeOffsets(fromNode.CoordinatesInGrid, (toNode.Vector3Position() - fromNode.Vector3Position()).To2D());
		Linecast.Ray2Nodes ray2Nodes = new Linecast.Ray2Nodes((GridGraph)fromNode.Graph, in offsets);
		NodeList occupiedNodes = target.GetOccupiedNodes();
		List<GridNodeBase> list = new List<GridNodeBase>();
		using Linecast.Ray2Nodes.Enumerator enumerator = ray2Nodes.GetEnumerator();
		while (enumerator.MoveNext())
		{
			GridNodeBase current = enumerator.Current;
			if (current == null || (list.Count == 0 && GraphHelper.GetWarhammerLength(current.CoordinatesInGrid - fromNode.CoordinatesInGrid) > range))
			{
				return (Nodes: TempList.Get<GridNodeBase>(), From: fromNode, To: toNode);
			}
			if (occupiedNodes.Contains(current) || list.Count > 0)
			{
				list.Add(enumerator.Current);
			}
		}
		return (Nodes: list, From: fromNode, To: toNode);
	}

	public static (List<GridNodeBase> Nodes, GridNodeBase From, GridNodeBase To) CollectNodes(GridNodeBase fromNode, MechanicEntity target, int range)
	{
		GridNodeBase gridNodeBase = target.GetOccupiedNodes().FirstOrDefault((GridNodeBase node) => LosCalculations.GetDirectLos(fromNode.Vector3Position(), node.Vector3Position())) ?? target.Position.GetNearestNodeXZUnwalkable();
		if (gridNodeBase == fromNode)
		{
			return (Nodes: TempList.Get<GridNodeBase>(), From: fromNode, To: gridNodeBase);
		}
		return CollectNodes(fromNode, gridNodeBase, target, range);
	}

	private static (GridNodeBase From, GridNodeBase To, List<GridNodeBase>[] Lines) CalculateLines(MechanicEntity caster, TargetWrapper target, AbilityExecutionContext context)
	{
		Cells cells = context.Ability.RangeCells.Cells();
		AbilityData ability = context.Ability;
		GridNodeBase casterNode = ability.GetBestShootingPositionForDesiredPosition(target);
		GridNodeBase gridNodeBase = ((!target.HasEntity) ? target.Point.GetNearestNodeXZUnwalkable() : (target.Entity.GetOccupiedNodes().FirstOrDefault((GridNodeBase node) => LosCalculations.GetDirectLos(casterNode.Vector3Position(), node.Vector3Position())) ?? target.Point.GetNearestNodeXZUnwalkable()));
		GridNodeBase obj = (ability.UseBestShootingPosition ? LosCalculations.GetBestShootingNode(casterNode, default(IntRect), gridNodeBase, default(IntRect)) : casterNode);
		List<GridNodeBase>[] item = GridPatterns.CalcScatterShot(obj, gridNodeBase, cells.Value);
		return (From: obj, To: gridNodeBase, Lines: item);
	}

	public void DisableAttacks()
	{
		AttacksDisabled = true;
	}

	public void DisableOverpenetration()
	{
		OverpenetrationDisabled = true;
	}

	public void DisableWeaponAttackDamage()
	{
		WeaponAttackDamageDisabled = true;
	}

	public void DisableDodgeForAlly()
	{
		DodgeForAllyDisabled = true;
	}

	public void AutoHit()
	{
		AttackHitPolicy = AttackHitPolicyType.AutoHit;
	}

	public void SetupLine(int index, GridNodeBase fromNode, GridNodeBase toNode, List<GridNodeBase> nodes)
	{
		if (nodes.Empty())
		{
			PFLog.Default.ErrorWithReport("Projectile path does not contains any of PriorityTarget's occupied nodes");
		}
		else
		{
			m_Attacks[index] = GetLine(index, fromNode, toNode, nodes);
		}
	}

	private AbilityProjectileAttackLine GetLine(int index, GridNodeBase fromNode, GridNodeBase toNode, List<GridNodeBase> nodes)
	{
		return new AbilityProjectileAttackLine(this, index, fromNode, toNode, nodes, WeaponAttackDamageDisabled, DodgeForAllyDisabled);
	}

	private IEnumerator<AbilityDeliveryTarget> CreateProcess()
	{
		while (true)
		{
			int i = 0;
			while (i < Math.Min(Context.ActionIndex, Count))
			{
				AbilityProjectileAttackLine line = this[i];
				if (line != null && !line.IsFinished)
				{
					while (line.Tick() && line.CurrentTarget != null)
					{
						yield return line.CurrentTarget;
					}
				}
				int num = i + 1;
				i = num;
			}
			IsFinished = m_Attacks.All((AbilityProjectileAttackLine i) => i == null || i.IsFinished);
			if (IsFinished)
			{
				break;
			}
			yield return null;
		}
	}

	public bool Tick()
	{
		if (IsFinished)
		{
			return false;
		}
		bool result = m_Process.MoveNext();
		CurrentTarget = m_Process.Current;
		return result;
	}

	bool IEnumerator.MoveNext()
	{
		return Tick();
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}

	[CanBeNull]
	public static MechanicEntity GetTargetByNode(GridNodeBase node)
	{
		BaseUnitEntity firstUnit = node.GetFirstUnit();
		if (firstUnit != null)
		{
			return firstUnit;
		}
		FilteredList<DestructibleEntity> destructibleEntities = node.GetDestructibleEntities();
		if (destructibleEntities.Any())
		{
			return destructibleEntities.First();
		}
		return null;
	}

	void IDisposable.Dispose()
	{
	}
}
