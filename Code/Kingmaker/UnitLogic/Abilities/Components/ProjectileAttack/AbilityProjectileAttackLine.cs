using System.Collections.Generic;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;

namespace Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;

public class AbilityProjectileAttackLine
{
	public struct HitData
	{
		[NotNull]
		public readonly GridNodeBase Node;

		[NotNull]
		public readonly RulePerformAttackRoll RollPerformAttackRule;

		[CanBeNull]
		public readonly MechanicEntity Entity;

		public LosDescription Los;

		public bool FromOverpenetration;

		public bool IsRedirecting;

		public bool Empty => Node == null;

		public HitData([NotNull] GridNodeBase node, RulePerformAttackRoll performAttackRoll, MechanicEntity entity)
		{
			this = default(HitData);
			Node = node;
			RollPerformAttackRule = performAttackRoll;
			Entity = entity;
		}
	}

	private readonly List<GridNodeBase> m_Nodes;

	private readonly IEnumerator<AbilityDeliveryTarget> m_DeliveryProcess;

	public readonly AbilityProjectileAttack ProjectileAttack;

	public readonly GridNodeBase FromNode;

	public readonly GridNodeBase ToNode;

	public readonly float StepHeight;

	public HitData[] Hits;

	public int Index { get; set; }

	public Projectile Projectile { get; set; }

	public bool IsFinished { get; private set; }

	public bool WeaponAttackDamageDisabled { get; private set; }

	public bool DodgeForAllyDisabled { get; private set; }

	public AbilityExecutionContext Context => ProjectileAttack.Context;

	[CanBeNull]
	public AbilityDeliveryTarget CurrentTarget => m_DeliveryProcess?.Current;

	public ReadonlyList<GridNodeBase> Nodes => m_Nodes;

	[CanBeNull]
	public MechanicEntity PriorityTarget => ProjectileAttack.PriorityTarget;

	public AbilityProjectileAttackLine(AbilityProjectileAttack projectileAttack, int index, GridNodeBase fromNode, GridNodeBase toNode, List<GridNodeBase> nodes, bool disableWeaponAttackDamage = false, bool disableDodgeForAlly = false)
	{
		Index = index;
		FromNode = fromNode;
		ToNode = toNode;
		m_Nodes = nodes;
		WeaponAttackDamageDisabled = disableWeaponAttackDamage;
		DodgeForAllyDisabled = disableDodgeForAlly;
		ProjectileAttack = projectileAttack;
		StepHeight = AbilityProjectileAttackLineHelper.GetStepHeightBetweenCells(FromNode, ToNode);
		BlueprintProjectile projectileBlueprint = Context.Ability?.ProjectileVariants.Random(PFStatefulRandom.UnitLogic.Abilities);
		m_DeliveryProcess = AbilityProjectileAttackLineHelper.DeliverLine(Context, projectileBlueprint, this);
	}

	public bool Tick()
	{
		bool flag = !IsFinished && (m_DeliveryProcess?.MoveNext() ?? false);
		IsFinished = !flag;
		return flag;
	}

	public (IEnumerable<HitData> Hits, bool HitCover) CalculateHits(bool autoHitFirst = false)
	{
		List<HitData> list = TempList.Get<HitData>();
		IAbilityAoEPatternProvider patternSettings = Context.Ability.Blueprint.PatternSettings;
		bool ignoreLos = patternSettings?.IsIgnoreLos ?? false;
		bool ignoreLevelDifference = patternSettings?.IsIgnoreLevelDifference ?? false;
		bool flag = false;
		bool flag2 = false;
		bool item = false;
		foreach (var target in AbilityProjectileAttackLineHelper.EnumerateTargets(Context, PriorityTarget, m_Nodes, FromNode, ToNode, ignoreLos, ignoreLevelDifference))
		{
			if ((Game.Instance.Controllers.TurnController.TurnBasedModeActive && !(target.Entity is BaseUnitEntity) && target.Entity != PriorityTarget && !target.Entity.CanBeAttackedDirectly) || list.Contains((HitData i) => i.Entity == target.Entity))
			{
				continue;
			}
			flag |= ProjectileAttack.IsControlledScatter && target.Entity.IsAlly(Context.Caster);
			RulePerformAttackRoll rulePerformAttackRoll = new RulePerformAttackRoll(Context.Caster, target.Entity, Context.Ability, Index, FromNode.Vector3Position(), target.Node.Vector3Position())
			{
				IsControlledScatterAutoMiss = flag
			};
			AttackHitPolicyType policy = (((autoHitFirst || flag2) && ProjectileAttack.AttackHitPolicy == AttackHitPolicyType.Default) ? AttackHitPolicyType.AutoHit : ProjectileAttack.AttackHitPolicy);
			using (ContextData<AttackHitPolicyContextData>.Request().Setup(policy))
			{
				Rulebook.Trigger(rulePerformAttackRoll);
			}
			bool isOverpenetration = rulePerformAttackRoll.IsOverpenetration;
			if (rulePerformAttackRoll.ResultIsCoverHit)
			{
				list.Add(new HitData(target.Los.Obstacle.Node, rulePerformAttackRoll, rulePerformAttackRoll.ResultCoverEntity)
				{
					FromOverpenetration = flag2
				});
				item = true;
				break;
			}
			list.Add(new HitData(target.Node, rulePerformAttackRoll, target.Entity)
			{
				FromOverpenetration = flag2
			});
			autoHitFirst = false;
			flag2 = false;
			if (!rulePerformAttackRoll.ResultIsHit)
			{
				break;
			}
			if (!rulePerformAttackRoll.RollPerformDefenceRule.IsDefended)
			{
				if (!isOverpenetration)
				{
					break;
				}
				flag2 = true;
			}
		}
		return (Hits: list, HitCover: item);
	}
}
