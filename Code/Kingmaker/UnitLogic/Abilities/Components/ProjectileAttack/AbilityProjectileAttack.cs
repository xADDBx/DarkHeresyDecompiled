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
	private readonly struct BurstDetails
	{
		public readonly MechanicEntity MainTarget;

		public readonly List<GridNodeBase> AffectedNodes;

		public readonly GridNodeBase FromNode;

		public readonly GridNodeBase ToNode;

		public readonly List<MechanicEntity> TargetsInPattern;

		public BurstDetails(MechanicEntity mainTarget, List<GridNodeBase> affectedNodes, GridNodeBase fromNode, GridNodeBase toNode, List<MechanicEntity> targetsInPattern)
		{
			MainTarget = mainTarget;
			AffectedNodes = affectedNodes;
			FromNode = fromNode;
			ToNode = toNode;
			TargetsInPattern = targetsInPattern;
		}
	}

	private readonly struct PatternBurstDetails
	{
		public readonly List<MechanicEntity> TargetsInPattern;

		public readonly GridNodeBase FromNode;

		public readonly GridNodeBase ToNode;

		public PatternBurstDetails(List<MechanicEntity> targetsInPattern, GridNodeBase fromNode, GridNodeBase toNode)
		{
			TargetsInPattern = targetsInPattern;
			FromNode = fromNode;
			ToNode = toNode;
		}
	}

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

	public static AbilityProjectileAttack CreatePatternBurst(AbilityExecutionContext context, GridNodeBase casterNode, TargetWrapper target, int shotsCount, bool controlledScatter)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		PatternBurstDetails patternBurstDetails = GetPatternBurstDetails(context, maybeCaster, target);
		AbilityProjectileAttack abilityProjectileAttack = new AbilityProjectileAttack(context, shotsCount, controlledScatter, patternBurstDetails.TargetsInPattern);
		int value = context.Ability.RangeCells.Cells().Value;
		List<GridNodeBase>[] list = GridPatterns.CalcScatterShot(patternBurstDetails.FromNode, patternBurstDetails.ToNode, value);
		for (int i = 0; i < abilityProjectileAttack.Count; i++)
		{
			if (!MakeSecondaryLine(i, abilityProjectileAttack, context, casterNode, patternBurstDetails.TargetsInPattern))
			{
				List<GridNodeBase> nodes = list.Random(PFStatefulRandom.UnitLogic.Abilities);
				AbilityProjectileAttackLine line = abilityProjectileAttack.GetLine(i, patternBurstDetails.FromNode, patternBurstDetails.ToNode, nodes);
				line.Hits = Array.Empty<AbilityProjectileAttackLine.HitData>();
				abilityProjectileAttack[i] = line;
			}
		}
		abilityProjectileAttack.RollCritEffectBodyPart();
		return abilityProjectileAttack;
	}

	public static AbilityProjectileAttack CreateBurst(AbilityExecutionContext context, GridNodeBase casterNode, TargetWrapper target, int shotsCount, bool controlledScatter)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		BurstDetails burstDetails = GetBurstDetails(context, casterNode, maybeCaster, target);
		AbilityProjectileAttack abilityProjectileAttack = new AbilityProjectileAttack(context, shotsCount, controlledScatter, burstDetails.TargetsInPattern);
		if (burstDetails.MainTarget == null)
		{
			PFLog.Default.Error("Scatter cant target point without unit!");
			return abilityProjectileAttack;
		}
		for (int num = 0; num < abilityProjectileAttack.Count; num++)
		{
			AbilityProjectileAttackLine line = abilityProjectileAttack.GetLine(num, burstDetails.FromNode, burstDetails.ToNode, burstDetails.AffectedNodes);
			(IEnumerable<AbilityProjectileAttackLine.HitData> Hits, bool HitCover) tuple = line.CalculateHits();
			IEnumerable<AbilityProjectileAttackLine.HitData> item = tuple.Hits;
			bool item2 = tuple.HitCover;
			line.Hits = item.ToArray();
			AbilityProjectileAttackLine.HitData? hitData = null;
			AbilityProjectileAttackLine.HitData[] hits = line.Hits;
			for (int i = 0; i < hits.Length; i++)
			{
				AbilityProjectileAttackLine.HitData value = hits[i];
				if (value.Entity == burstDetails.MainTarget)
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
					goto IL_010c;
				}
			}
			num2 = 0;
			goto IL_010c;
			IL_010c:
			if (((uint)num2 | (item2 ? 1u : 0u)) == 0)
			{
				List<MechanicEntity> list = burstDetails.TargetsInPattern.ToTempList();
				list.Remove(burstDetails.MainTarget);
				MakeSecondaryLine(num, abilityProjectileAttack, context, casterNode, list);
			}
		}
		abilityProjectileAttack.RollCritEffectBodyPart();
		return abilityProjectileAttack;
	}

	public static List<MechanicEntity> GetPatternBurstTargets(AbilityExecutionContext context, TargetWrapper target)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		return GetPatternBurstDetails(context, maybeCaster, target).TargetsInPattern;
	}

	public static List<MechanicEntity> GetBurstTargets(AbilityExecutionContext context, GridNodeBase casterNode, TargetWrapper target)
	{
		MechanicEntity maybeCaster = context.MaybeCaster;
		if (maybeCaster == null)
		{
			PFLog.Default.Error("Caster is missing");
			return null;
		}
		return GetBurstDetails(context, casterNode, maybeCaster, target).TargetsInPattern;
	}

	private static PatternBurstDetails GetPatternBurstDetails(AbilityExecutionContext context, MechanicEntity caster, TargetWrapper target)
	{
		AbilityData ability = context.Ability;
		GridNodeBase bestShootingNode = ability.GetBestShootingPositionForDesiredPosition(target);
		GridNodeBase gridNodeBase = ((!target.HasEntity) ? target.Point.GetNearestNodeXZUnwalkable() : (target.Entity.GetOccupiedNodes().FirstOrDefault((GridNodeBase node) => LosCalculations.GetDirectLos(bestShootingNode.Vector3Position(), node.Vector3Position())) ?? target.Point.GetNearestNodeXZUnwalkable()));
		GridNodeBase gridNodeBase2 = (ability.UseBestShootingPosition ? LosCalculations.GetBestShootingNode(bestShootingNode, default(IntRect), gridNodeBase, default(IntRect)) : bestShootingNode);
		return new PatternBurstDetails((from x in AbilityProjectileAttackLineHelper.EnumerateTargets(context, null, context.Ability.Blueprint.PatternSettings.GetOrientedPattern(context.Ability, gridNodeBase2, gridNodeBase, caster.Size, coveredTargetsOnly: true).Nodes, gridNodeBase2, gridNodeBase, context.Ability.Blueprint.PatternSettings.IsIgnoreLos, context.Ability.Blueprint.PatternSettings.IsIgnoreLevelDifference)
			select x.Entity).ToTempList(), gridNodeBase2, gridNodeBase);
	}

	private static BurstDetails GetBurstDetails(AbilityExecutionContext context, GridNodeBase casterNode, MechanicEntity caster, TargetWrapper target)
	{
		MechanicEntity mechanicEntity = target.Entity ?? target.NearestNode.GetFirstUnit();
		(List<GridNodeBase> Nodes, GridNodeBase From, GridNodeBase To) singleShotAffectedNodes = GetSingleShotAffectedNodes(context.Ability, casterNode, mechanicEntity);
		List<GridNodeBase> item = singleShotAffectedNodes.Nodes;
		GridNodeBase item2 = singleShotAffectedNodes.From;
		GridNodeBase item3 = singleShotAffectedNodes.To;
		List<MechanicEntity> targetsInPattern = (from x in AbilityProjectileAttackLineHelper.EnumerateTargets(context, mechanicEntity, context.Ability.Blueprint.PatternSettings.GetOrientedPattern(context.Ability, casterNode, item3, caster.Size, coveredTargetsOnly: true).Nodes, item2, item3)
			select x.Entity).ToTempList();
		return new BurstDetails(mechanicEntity, item, item2, item3, targetsInPattern);
	}

	private static bool MakeSecondaryLine(int index, AbilityProjectileAttack attack, AbilityExecutionContext context, GridNodeBase casterNode, List<MechanicEntity> unitsInPattern)
	{
		MechanicEntity resultTarget = Rulebook.Trigger(RulePerformTargetInBurstSelection.Setup(context.Caster).WithTargets(unitsInPattern).Create()).ResultTarget;
		if (resultTarget == null)
		{
			return false;
		}
		(List<GridNodeBase>, GridNodeBase, GridNodeBase) singleShotAffectedNodes = GetSingleShotAffectedNodes(context.Ability, casterNode, resultTarget);
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

	public static AbilityProjectileAttack CreateSingleTarget(AbilityExecutionContext context, GridNodeBase casterNode, MechanicEntity priorityTarget, int shotsCount)
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
		(List<GridNodeBase>, GridNodeBase, GridNodeBase) singleShotAffectedNodes = GetSingleShotAffectedNodes(context.Ability, casterNode, priorityTarget);
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

	public static (List<GridNodeBase> Nodes, GridNodeBase From, GridNodeBase To) GetSingleShotAffectedNodes(AbilityData ability, GridNodeBase casterNode, MechanicEntity target)
	{
		bool ignoreLos = ability.Blueprint.PatternSettings?.IsIgnoreLos ?? false;
		int num = ability.RangeCells.Cells().Value;
		var (gridNodeBase, gridNodeBase2) = GetFromToNodes(ability, casterNode, target, ignoreLos);
		if (gridNodeBase2 == gridNodeBase)
		{
			return (Nodes: TempList.Get<GridNodeBase>(), From: gridNodeBase, To: gridNodeBase2);
		}
		if (gridNodeBase2 != null)
		{
			int warhammerLength = GraphHelper.GetWarhammerLength(gridNodeBase2.CoordinatesInGrid - gridNodeBase.CoordinatesInGrid);
			if (warhammerLength > num && WarhammerGeometryUtils.DistanceToInCells(gridNodeBase.Vector3Position(), ability.Caster.SizeRect, target.Position, target.SizeRect) <= num)
			{
				num = warhammerLength;
			}
		}
		return CollectNodes(gridNodeBase, gridNodeBase2, target, num);
	}

	private static (GridNodeBase From, GridNodeBase To) GetFromToNodes(AbilityData ability, GridNodeBase casterNode, MechanicEntity target, bool ignoreLos)
	{
		GridNodeBase item = null;
		GridNodeBase item2 = null;
		int num = int.MaxValue;
		foreach (GridNodeBase occupiedNode in ability.Caster.GetOccupiedNodes(casterNode))
		{
			foreach (GridNodeBase occupiedNode2 in target.GetOccupiedNodes())
			{
				if (ignoreLos || LosCalculations.GetDirectLos(occupiedNode.Vector3Position(), occupiedNode2.Vector3Position()))
				{
					int num2 = WarhammerGeometryUtils.DistanceToInCells(occupiedNode.CoordinatesInGrid, default(IntRect), occupiedNode2.CoordinatesInGrid, default(IntRect));
					if (num2 < num)
					{
						item = occupiedNode;
						item2 = occupiedNode2;
						num = num2;
					}
				}
			}
		}
		return (From: item, To: item2);
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

	void IDisposable.Dispose()
	{
	}
}
