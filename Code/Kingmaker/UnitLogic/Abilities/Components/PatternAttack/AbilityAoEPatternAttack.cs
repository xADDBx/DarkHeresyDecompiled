using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers;
using Kingmaker.Controllers.Projectiles;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Groups;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.Random;
using Kingmaker.Utility.UnityExtensions;
using Pathfinding;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.UnitLogic.Abilities.Components.PatternAttack;

public class AbilityAoEPatternAttack : IEnumerator<AbilityDeliveryTarget>, IEnumerator, IDisposable
{
	private readonly IEnumerator<AbilityDeliveryTarget> m_Process;

	public AbilityExecutionContext Context { get; }

	public IAbilityAoEPatternProvider Settings { get; }

	public AbilitySpawnAreaEffectSettings SpawnAreaEffect { get; }

	public bool IsWeaponAttack { get; }

	public int AdditionalDamageInstancesCount { get; }

	public bool IsFinished { get; private set; }

	public AbilityDeliveryTarget CurrentTarget { get; private set; }

	public bool WeaponAttackDamageDisabled { get; private set; }

	public bool DodgeForAllyDisabled { get; private set; }

	AbilityDeliveryTarget IEnumerator<AbilityDeliveryTarget>.Current => CurrentTarget;

	object IEnumerator.Current => CurrentTarget;

	public AbilityAoEPatternAttack(AbilityExecutionContext context, AbilityAttackDelivery attackSettings, IAbilityAoEPatternProvider patternSettings, AbilitySpawnAreaEffectSettings spawnAreaEffect, GridNodeBase fromNode, GridNodeBase toNode)
	{
		Context = context;
		Settings = patternSettings;
		IsWeaponAttack = attackSettings.IsWeaponAttack;
		AdditionalDamageInstancesCount = attackSettings.AdditionalDamageInstancesCount;
		SpawnAreaEffect = spawnAreaEffect;
		m_Process = Deliver(context, attackSettings, fromNode, toNode);
		WeaponAttackDamageDisabled = !IsWeaponAttack;
	}

	private IEnumerator<AbilityDeliveryTarget> Deliver(AbilityExecutionContext context, AbilityAttackDelivery attackSettings, GridNodeBase fromNode, GridNodeBase toNode)
	{
		if (SpawnAreaEffect.SpawnImmediately)
		{
			TrySpawnAreaEffect(context, context.ClickedTarget);
		}
		IEnumerator<AbilityDeliveryTarget> process = CreateDeliveryProcess(context, attackSettings, fromNode, toNode);
		while (process.MoveNext())
		{
			yield return process.Current;
		}
		if (!SpawnAreaEffect.SpawnImmediately)
		{
			TrySpawnAreaEffect(context, context.ClickedTarget);
		}
	}

	private IEnumerator<AbilityDeliveryTarget> CreateDeliveryProcess(AbilityExecutionContext context, AbilityAttackDelivery attackSettings, GridNodeBase fromNode, GridNodeBase toNode)
	{
		BlueprintProjectile blueprintProjectile = context.Ability.ProjectileVariants.Random(PFStatefulRandom.UnitLogic.Abilities);
		if (context.AbilityBlueprint.IsCustomProjectileDistribution)
		{
			return DeliverWithCustomProjectileDistribution(context, blueprintProjectile);
		}
		if (!attackSettings.IsRanged && !attackSettings.IsThrow)
		{
			return DeliverDirectly(context);
		}
		if (attackSettings.PatternSpreadWithProjectile)
		{
			return SpreadWithProjectile(context, fromNode, context.Pattern.Nodes.Last(), blueprintProjectile, attackSettings);
		}
		return DeliverWithProjectile(context, fromNode, toNode, blueprintProjectile, attackSettings);
	}

	private AbilityDeliveryTarget DeliverHit(AbilityExecutionContext context, MechanicEntity target, OrientedPatternData pattern, [CanBeNull] Projectile projectile = null, Vector3? effectiveCasterPosition = null, bool autoHit = true)
	{
		MechanicEntity caster = context.Caster;
		AbilityData ability = context.Ability;
		bool weaponAttackDamageDisabled = WeaponAttackDamageDisabled;
		bool dodgeForAllyDisabled = DodgeForAllyDisabled;
		AttackHitPolicyType attackHitPolicy = (autoHit ? AttackHitPolicyType.AutoHit : AttackHitPolicyType.Default);
		RulePerformAttack rulePerformAttack = new RulePerformAttack(caster, target, ability, 0, weaponAttackDamageDisabled, dodgeForAllyDisabled, effectiveCasterPosition, null, useSpecificAttackHitPolicy: true, attackHitPolicy)
		{
			AdditionalDamageInstancesCount = AdditionalDamageInstancesCount,
			Reason = context
		};
		rulePerformAttack.RollPerformAttackRule.DangerArea.UnionWith(pattern.Nodes);
		rulePerformAttack.RollPerformAttackRule.CanApplyCriticalEffect = true;
		context.TriggerRule(rulePerformAttack);
		return new AbilityDeliveryTarget(target)
		{
			AttackRule = rulePerformAttack,
			Projectile = projectile
		};
	}

	private IEnumerator<AbilityDeliveryTarget> SpreadWithProjectile(AbilityExecutionContext context, GridNodeBase fromNode, GridNodeBase toNode, BlueprintProjectile blueprintProjectile, AbilityAttackDelivery attackSettings)
	{
		Debug.DrawLine(fromNode.Vector3Position(), toNode.Vector3Position(), Color.yellow);
		OrientedPatternData pattern = context.Pattern;
		HashSet<MechanicEntity> targets;
		using (CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Get(out targets))
		{
			foreach (MechanicEntity targetableEntity in Game.Instance.EntityPools.TargetableEntities)
			{
				if (context.Ability.IsValidTargetForAttack(targetableEntity) && targetableEntity != context.Caster && AoEPatternHelper.WouldTargetEntity(pattern, targetableEntity))
				{
					targets.Add(targetableEntity);
				}
			}
			context.TargetsInPatternCount = targets.Count;
			Vector3 castPosition = fromNode.Vector3Position();
			Vector3 vector = toNode.Vector3Position();
			TargetWrapper launcher = new TargetWrapper(castPosition, null, context.Caster);
			Projectile projectile = new ProjectileLauncher(blueprintProjectile, launcher, vector).Ability(context.Ability).MaxRangeCells(context.Ability.RangeCells).Index(0)
				.Launch();
			float distance = projectile.Distance(fromNode.Vector3Position(), toNode.Vector3Position());
			HashSet<MechanicEntity> usedTargets;
			using (CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Get(out usedTargets))
			{
				do
				{
					yield return null;
					Debug.DrawLine(fromNode.Vector3Position(), toNode.Vector3Position(), Color.yellow);
					if (projectile.Cleared)
					{
						break;
					}
					foreach (MechanicEntity item in targets)
					{
						float distance2 = ((!projectile.IsHit) ? projectile.PassedDistance : float.MaxValue);
						if (AoEPatternHelper.WouldTargetEntityPattern(item, pattern, castPosition, distance2, out var _) && usedTargets.Add(item))
						{
							yield return DeliverSpreadWithProjectileHit(context, item, pattern, projectile, castPosition);
						}
					}
				}
				while (!projectile.IsEnoughTimePassedToTraverseDistance(distance));
			}
		}
	}

	private AbilityDeliveryTarget DeliverSpreadWithProjectileHit(AbilityExecutionContext context, MechanicEntity target, OrientedPatternData pattern, [CanBeNull] Projectile projectile, Vector3 castPosition)
	{
		Debug.DrawLine(target.Position, target.Position + Vector3.up * 3f, Color.red);
		Vector3 value = (Settings.IsIgnoreLevelDifference ? target.Position : castPosition);
		return DeliverHit(context, target, pattern, projectile, value);
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverWithProjectile(AbilityExecutionContext context, GridNodeBase fromNode, GridNodeBase toNode, BlueprintProjectile blueprintProjectile, AbilityAttackDelivery attackSettings)
	{
		Vector3 point = fromNode.Vector3Position();
		Vector3 vector = toNode.Vector3Position();
		Projectile projectile = null;
		if (blueprintProjectile != null)
		{
			TargetWrapper launcher = new TargetWrapper(point, null, context.Caster);
			projectile = new ProjectileLauncher(blueprintProjectile, launcher, vector).Ability(context.Ability).MaxRangeCells(context.Ability.RangeCells).Index(0)
				.Launch();
			float distance = projectile.Distance(fromNode.Vector3Position(), toNode.Vector3Position());
			do
			{
				yield return null;
				if (projectile.Cleared)
				{
					yield break;
				}
			}
			while (!projectile.IsEnoughTimePassedToTraverseDistance(distance));
		}
		OrientedPatternData pattern = context.Pattern;
		Vector3 patternApplicationPosition = context.Pattern.ApplicationNode.Vector3Position();
		MechanicEntity[] array = (from i in Game.Instance.EntityPools.TargetableEntities
			where IsValidTarget(context, i, checkTargetType: false)
			orderby Vector3.Distance(patternApplicationPosition, i.Position)
			select i).ToArray();
		context.TargetsInPatternCount = array.Length;
		MechanicEntity[] array2 = array;
		foreach (MechanicEntity target in array2)
		{
			yield return DeliverHit(context, target, pattern, projectile);
		}
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverDirectly(AbilityExecutionContext context)
	{
		HashSet<MechanicEntity> value;
		using (CollectionPool<HashSet<MechanicEntity>, MechanicEntity>.Get(out value))
		{
			OrientedPatternData pattern = context.Pattern;
			foreach (MechanicEntity targetableEntity in Game.Instance.EntityPools.TargetableEntities)
			{
				if (IsValidTarget(context, targetableEntity, checkTargetType: true))
				{
					value.Add(targetableEntity);
				}
			}
			context.TargetsInPatternCount = value.Count;
			foreach (MechanicEntity item in value)
			{
				yield return DeliverHit(context, item, pattern, null, null, autoHit: false);
			}
		}
	}

	private IEnumerator<AbilityDeliveryTarget> DeliverWithCustomProjectileDistribution(AbilityExecutionContext context, BlueprintProjectile blueprintProjectile)
	{
		CustomProjectileDistribution component = context.AbilityBlueprint.GetComponent<CustomProjectileDistribution>();
		if (component == null)
		{
			yield break;
		}
		OrientedPatternData pattern = context.Pattern;
		List<MechanicEntity> list;
		using (Game.Instance.EntityPools.TargetableEntities.Where((MechanicEntity i) => IsValidTarget(context, i, checkTargetType: true)).ToPooledList(out list))
		{
			context.TargetsInPatternCount = list.Count;
			List<Projectile> projectiles;
			using ((from x in component.Launch(context, blueprintProjectile, list)
				where !x.DoNotDeliverHit
				select x).ToPooledList(out projectiles))
			{
				do
				{
					yield return null;
					for (int index = projectiles.Count - 1; index >= 0; index--)
					{
						Projectile projectile = projectiles[index];
						bool num = projectile.IsEnoughTimePassedToTraverseDistance();
						bool cleared = projectile.Cleared;
						if (num || cleared)
						{
							projectiles.Remove(projectile);
						}
						if (num)
						{
							yield return DeliverHit(context, projectile.Target.Entity, pattern);
						}
					}
				}
				while (projectiles.Count > 0);
			}
		}
	}

	public static bool IsValidTarget(AbilityExecutionContext context, MechanicEntity entity, bool checkTargetType)
	{
		if (!context.Ability.IsValidTargetForAttack(entity))
		{
			return false;
		}
		if ((context.Ability.IsMelee || context.Ability.IsBurst) && entity == context.Caster)
		{
			return false;
		}
		if (!AoEPatternHelper.WouldTargetEntity(context.Pattern, entity))
		{
			return false;
		}
		if (!checkTargetType)
		{
			return true;
		}
		IAbilityAoEPatternProvider patternSettings = context.Ability.GetPatternSettings();
		if (patternSettings.Targets == TargetType.Any)
		{
			return true;
		}
		PartCombatGroup combatGroupOptional = entity.GetCombatGroupOptional();
		switch (patternSettings.Targets)
		{
		case TargetType.Enemy:
			if (combatGroupOptional == null || combatGroupOptional.IsEnemy(context.Caster))
			{
				break;
			}
			goto IL_00a7;
		case TargetType.Ally:
			{
				if (combatGroupOptional != null && combatGroupOptional.IsAlly(context.Caster))
				{
					break;
				}
				goto IL_00a7;
			}
			IL_00a7:
			return false;
		}
		return true;
	}

	public void TrySpawnAreaEffect(AbilityExecutionContext context, TargetWrapper target)
	{
		BlueprintAreaEffect blueprint = SpawnAreaEffect.Blueprint;
		if (blueprint != null)
		{
			TimeSpan seconds = SpawnAreaEffect.DurationValue.Calculate(context).Seconds;
			AreaEffectsController.CreateSpawner(blueprint, context, target).Duration(seconds).UsePatternFromAbility(SpawnAreaEffect.UseAttackPattern)
				.GetOrientationFromCaster(SpawnAreaEffect.GetOrientationFromCaster)
				.Spawn();
		}
	}

	public void DisableWeaponAttackDamage()
	{
		WeaponAttackDamageDisabled = true;
	}

	public void DisableDodgeForAlly()
	{
		DodgeForAllyDisabled = true;
	}

	void IEnumerator.Reset()
	{
		throw new NotImplementedException();
	}

	bool IEnumerator.MoveNext()
	{
		if (IsFinished)
		{
			return false;
		}
		IsFinished = !m_Process.MoveNext();
		CurrentTarget = (IsFinished ? null : m_Process.Current);
		return !IsFinished;
	}

	void IDisposable.Dispose()
	{
	}
}
