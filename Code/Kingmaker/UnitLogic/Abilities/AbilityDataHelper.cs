using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Gameplay.ContextActions;
using Kingmaker.Items;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Pathfinding;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Conditions;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

public static class AbilityDataHelper
{
	public static bool HasIsAllyCondition(this ConditionsChecker conditionsChecker)
	{
		if (conditionsChecker.HasConditions)
		{
			Condition[] conditions = conditionsChecker.Conditions;
			for (int i = 0; i < conditions.Length; i++)
			{
				if (conditions[i] is ContextConditionIsAlly)
				{
					return true;
				}
			}
		}
		return false;
	}

	public static bool IsValidTargetForAttack(this AbilityData ability, MechanicEntity target)
	{
		if (target.GetHealthOptional() == null)
		{
			return false;
		}
		PartLifeState lifeStateOptional = target.GetLifeStateOptional();
		if (lifeStateOptional != null && !lifeStateOptional.IsConscious && !ability.Blueprint.CanCastToDeadTarget)
		{
			return false;
		}
		if (ability.Caster.IsAttackingGreenNPC(target))
		{
			return false;
		}
		if ((bool)ability.Caster.Features.AttacksIgnoreAllies && ability.Caster.IsAlly(target))
		{
			return false;
		}
		bool flag = !ability.Blueprint.CanTargetDestructibleObjects;
		if (flag)
		{
			bool flag2 = ((target is DestructibleEntity || (target != null && target.IsMechanism)) ? true : false);
			flag = flag2;
		}
		if (flag)
		{
			return false;
		}
		if (ability.IsAoe && (bool)target.Features.IgnoreAlliedAoeAttacks && ability.Caster.IsAlly(target))
		{
			return false;
		}
		if (AbstractUnitCommand.CommandTargetUntargetable(ability.Caster, target))
		{
			return false;
		}
		return true;
	}

	public static bool IsPatternRestrictionPassed(this AbilityData abilityData, TargetWrapper target)
	{
		return abilityData.Blueprint?.GetComponent<IAbilityPatternRestriction>()?.IsPatternRestrictionPassed(abilityData, abilityData.Caster, target) ?? true;
	}

	public static bool IsResultPatternEmpty(this AbilityData abilityData, TargetWrapper target)
	{
		if (abilityData.Blueprint?.PatternSettings?.Pattern == null)
		{
			return false;
		}
		IAbilityAoEPatternProvider patternSettings = abilityData.GetPatternSettings();
		GridNodeBase bestShootingPositionForDesiredPosition = abilityData.GetBestShootingPositionForDesiredPosition(target);
		GridNodeBase actualCastNode = AoEPatternHelper.GetActualCastNode(abilityData.Caster, bestShootingPositionForDesiredPosition, target.Point, abilityData.MinRangeCells, abilityData.RangeCells);
		Size targetSizeForPattern = abilityData.GetTargetSizeForPattern(target);
		return patternSettings.GetOrientedPattern(abilityData, bestShootingPositionForDesiredPosition, actualCastNode, targetSizeForPattern).Nodes.IsEmpty;
	}

	public static List<GridNodeBase> GetSingleShotAffectedNodes(this AbilityData ability, TargetWrapper target)
	{
		if (!ability.IsSingleTarget || target?.Entity == null)
		{
			return TempList.Get<GridNodeBase>();
		}
		return AbilityProjectileAttack.GetSingleShotAffectedNodes(ability, target.Entity).Nodes;
	}

	public static bool IsChainLightning(this AbilityData ability)
	{
		return ability.Blueprint.GetComponent<AbilityDeliverChain>() != null;
	}

	public static bool HasAttackTypeProvider(this AbilityData ability)
	{
		return ability.Blueprint.GetComponent<IAbilityAttackTypeProvider>() != null;
	}

	public static bool HasCustomDirectMovement(this AbilityData ability)
	{
		return ability.Blueprint.GetComponent<AbilityCustomDirectMovement>() != null;
	}

	public static HashSet<GridNodeBase> GetChainLightingTargets(this AbilityData ability, TargetWrapper target)
	{
		if (ability?.Caster == null)
		{
			return TempHashSet.Get<GridNodeBase>();
		}
		MechanicEntity mechanicEntity = target?.Entity;
		if (mechanicEntity == null)
		{
			return TempHashSet.Get<GridNodeBase>();
		}
		AbilityDeliverChain component = ability.Blueprint.GetComponent<AbilityDeliverChain>();
		if (component == null)
		{
			return TempHashSet.Get<GridNodeBase>();
		}
		HashSet<MechanicEntity> hashSet = new HashSet<MechanicEntity> { mechanicEntity };
		int value = component.TargetsCount.Value;
		int num = 0;
		while (num < value)
		{
			num++;
			if (num < value)
			{
				mechanicEntity = SelectNextTarget(ability, component, target, hashSet);
				if (mechanicEntity != null)
				{
					hashSet.Add(mechanicEntity);
				}
			}
		}
		return hashSet.SelectMany((MechanicEntity u) => u.GetOccupiedNodes()).ToTempHashSet();
	}

	public static PartAbilityPredictionForAreaEffect TryGetPatternDataFromAreaEffect(this AbilityData abilityData)
	{
		return abilityData.Caster.GetPartAbilityPredictionForAreaEffectOptional();
	}

	private static BaseUnitEntity SelectNextTarget(AbilityData abilityData, AbilityDeliverChain chainComponent, TargetWrapper originalTarget, HashSet<MechanicEntity> usedTargets)
	{
		Vector3 point = originalTarget.Point;
		float num = float.MaxValue;
		BaseUnitEntity result = null;
		foreach (BaseUnitEntity allBaseUnit in Game.Instance.EntityPools.AllBaseUnits)
		{
			if (abilityData.IsValidTargetForAttack(allBaseUnit))
			{
				float num2 = allBaseUnit.DistanceToInCells(point);
				if (CheckTarget(abilityData, chainComponent, allBaseUnit) && num2 <= (float)chainComponent.Radius && !usedTargets.Contains(allBaseUnit) && num2 < num)
				{
					num = num2;
					result = allBaseUnit;
				}
			}
		}
		return result;
	}

	private static bool CheckTarget(AbilityData abilityData, AbilityDeliverChain chainComponent, BaseUnitEntity unit)
	{
		if (abilityData.Caster == null)
		{
			return false;
		}
		if (unit.LifeState.IsDead && !chainComponent.TargetDead)
		{
			return false;
		}
		if ((chainComponent.TargetType == TargetType.Enemy && !abilityData.Caster.IsEnemy(unit)) || (chainComponent.TargetType == TargetType.Ally && abilityData.Caster.IsEnemy(unit)))
		{
			return false;
		}
		return true;
	}

	public static HealPredictionData GetHealPrediction(this AbilityData ability, MechanicEntity target)
	{
		AbilityEffectRunAction component = ability.Blueprint.GetComponent<AbilityEffectRunAction>();
		if (component == null)
		{
			return null;
		}
		try
		{
			return GetActionsHeal(ability, null, component.Actions, target);
		}
		catch (Exception ex)
		{
			LogChannel.Default.Error(ex);
		}
		return null;
	}

	private static HealPredictionData GetActionsHeal(AbilityData ability, [CanBeNull] AbilityExecutionContext context, ActionList actions, MechanicEntity target)
	{
		HealPredictionData healPredictionData = null;
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction action in actions2)
		{
			if (context == null)
			{
				context = ability.ClaimExecutionContext(target, ability.Caster.Position);
			}
			HealPredictionData healPredictionData2 = null;
			healPredictionData2 = TryGetHealPredictionFromAction(ability, context, target, action);
			if (healPredictionData2 != null)
			{
				if (healPredictionData == null)
				{
					healPredictionData = new HealPredictionData();
				}
				healPredictionData += healPredictionData2;
			}
		}
		return healPredictionData;
	}

	private static HealPredictionData TryGetHealPredictionFromAction(AbilityData ability, AbilityExecutionContext context, MechanicEntity target, GameAction action)
	{
		if (action is Conditional conditional)
		{
			bool flag = false;
			using (context.SetScope(target.ToITargetWrapper()))
			{
				flag = conditional.ConditionsChecker.Check();
			}
			if (!flag)
			{
				return GetActionsHeal(ability, context, conditional.IfFalse, target);
			}
			return GetActionsHeal(ability, context, conditional.IfTrue, target);
		}
		if (action is ContextActionHealTarget contextActionHealTarget)
		{
			return contextActionHealTarget.GetHealPrediction(context, target);
		}
		return null;
	}

	public static IntermediateDamage GetWeaponDamageData(this AbilityData ability, MechanicEntity target)
	{
		using (ProfileScope.New("AbilityDataHelper > WarhammerAbilityAttackDelivery"))
		{
			if (!ability.Blueprint.GetComponent<AbilityAttackDelivery>() && ability.Weapon != null)
			{
				return null;
			}
			int enemyTargetCountInPattern = GetEnemyTargetCountInPattern(ability);
			using (ContextData<EnemyTargetsInPatternData>.Request().Setup(enemyTargetCountInPattern))
			{
				return Rulebook.Trigger(new RuleCalculateDamage(ability.Caster, target, ability, null, null, ability.PreciseBodyPart)).ResultDamage;
			}
		}
	}

	public static DamagePredictionData GetDamagePrediction(this AbilityData ability, [CanBeNull] MechanicEntity target, Vector3 casterPosition, [CanBeNull] IntermediateDamage damage, AbilityExecutionContext context = null)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			DamagePredictionData damagePrediction = ((damage == null) ? new DamagePredictionData() : new DamagePredictionData
			{
				MinDamage = damage.MinValue,
				MaxDamage = damage.MaxValue,
				HPDamageBonus = damage.BonusDamageToHealth,
				ArmorDamageBonus = damage.ArmorDamageModifiers.Value,
				VitalDamage = damage.VitalDamage
			});
			damagePrediction = ApplyActionEffects(damagePrediction, ability, target, casterPosition, context);
			return (damagePrediction.MaxDamage == 0) ? null : damagePrediction;
		}
	}

	public static DamagePredictionData GetDamagePrediction(this AbilityData ability, [CanBeNull] MechanicEntity target, Vector3 casterPosition, AbilityExecutionContext context = null)
	{
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
			DamagePredictionData damagePrediction = new DamagePredictionData
			{
				MinDamage = 0,
				MaxDamage = 0,
				HPDamageBonus = 0,
				ArmorDamageBonus = 0,
				VitalDamage = 0
			};
			using (ProfileScope.New("AbilityDataHelper > WarhammerAbilityAttackDelivery"))
			{
				if ((bool)ability.Blueprint.GetComponent<AbilityAttackDelivery>() && ability.Weapon != null)
				{
					int enemyTargetCountInPattern = GetEnemyTargetCountInPattern(ability);
					using (ContextData<EnemyTargetsInPatternData>.Request().Setup(enemyTargetCountInPattern))
					{
						IntermediateDamage resultDamage = Rulebook.Trigger(new RuleCalculateDamage(ability.Caster, target, ability, null, null, ability.PreciseBodyPart)).ResultDamage;
						if (resultDamage == null)
						{
							Debug.LogError("Weapon calculate damage is broken: RuleCalculateDamage == NULL");
							return null;
						}
						DamagePredictionData damagePredictionData = new DamagePredictionData
						{
							MinDamage = resultDamage.MinValue,
							MaxDamage = resultDamage.MaxValue,
							HPDamageBonus = resultDamage.BonusDamageToHealth,
							ArmorDamageBonus = resultDamage.ArmorDamageModifiers.Value,
							VitalDamage = resultDamage.VitalDamage
						};
						damagePrediction += damagePredictionData;
					}
				}
			}
			damagePrediction = ApplyActionEffects(damagePrediction, ability, target, casterPosition, context);
			return (damagePrediction.MaxDamage == 0) ? null : damagePrediction;
		}
	}

	public static DamagePredictionData ApplyActionEffects(DamagePredictionData damagePrediction, AbilityData ability, [CanBeNull] MechanicEntity target, Vector3 casterPosition, AbilityExecutionContext context)
	{
		using (ProfileScope.New("AbilityDataHelper > AbilityEffectRunAction"))
		{
			foreach (AbilityEffectRunAction component in ability.Blueprint.GetComponents<AbilityEffectRunAction>())
			{
				try
				{
					DamagePredictionData actionsDamage = GetActionsDamage(ability, component.Actions, context, casterPosition, target ?? Game.Instance.DefaultUnit);
					damagePrediction += actionsDamage;
					ActionList actions = (ability.Caster.IsAlly(target) ? component.ActionsOnAlly : component.ActionsOnEnemy);
					DamagePredictionData actionsDamage2 = GetActionsDamage(ability, actions, context, casterPosition, target ?? Game.Instance.DefaultUnit);
					damagePrediction += actionsDamage2;
				}
				catch (Exception ex)
				{
					LogChannel.Default.Error(ex);
				}
			}
			return damagePrediction;
		}
	}

	private static int GetEnemyTargetCountInPattern(AbilityData abilityData)
	{
		if (abilityData == null)
		{
			return 0;
		}
		PointerController pointerController = Game.Instance.Controllers.PointerController;
		ClickWithSelectedAbilityHandler clickWithSelectedAbilityHandler = pointerController?.GetHandler<ClickWithSelectedAbilityHandler>();
		if (pointerController == null || clickWithSelectedAbilityHandler == null)
		{
			return 0;
		}
		TargetWrapper target = clickWithSelectedAbilityHandler.GetTarget(pointerController.PointerOn, pointerController.WorldPosition, abilityData, abilityData.Caster.Position);
		target = ((target != null) ? target : ((TargetWrapper)pointerController.WorldPosition));
		OrientedPatternData pattern = abilityData.GetPattern(target, abilityData.Caster.Position);
		int num = 0;
		foreach (GridNodeBase node in pattern.Nodes)
		{
			BaseUnitEntity firstUnit = node.GetFirstUnit();
			if (firstUnit != null && firstUnit != abilityData.Caster && !firstUnit.IsAlly(abilityData.Caster))
			{
				num++;
			}
		}
		return num;
	}

	private static DamagePredictionData GetActionsDamage([NotNull] AbilityData ability, [NotNull] ActionList actions, [CanBeNull] AbilityExecutionContext context, Vector3 casterPosition, [NotNull] TargetWrapper target)
	{
		DamagePredictionData damagePredictionData = null;
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction gameAction in actions2)
		{
			DamagePredictionData damagePredictionData2 = null;
			if (gameAction is Conditional conditional)
			{
				if (context == null)
				{
					context = ability.ClaimExecutionContext(target, casterPosition);
				}
				using (context.SetScope(target))
				{
					bool flag = false;
					using (context.SetScope(target))
					{
						flag = conditional.ConditionsChecker.Check();
					}
					damagePredictionData2 = (flag ? GetActionsDamage(ability, conditional.IfTrue, context, casterPosition, target) : GetActionsDamage(ability, conditional.IfFalse, context, casterPosition, target));
				}
			}
			else if (gameAction is ContextActionDealDamage contextActionDealDamage)
			{
				if (context == null)
				{
					context = ability.ClaimExecutionContext(target, casterPosition);
				}
				using (context.SetScope(target))
				{
					damagePredictionData2 = contextActionDealDamage.GetDamagePrediction(context, target.Entity);
				}
			}
			else if (gameAction is ContextActionSavingThrow contextActionSavingThrow)
			{
				if (context == null)
				{
					context = ability.ClaimExecutionContext(target, casterPosition);
				}
				using (context.SetScope(target))
				{
					DamagePredictionData actionsDamage;
					using (ContextData<SavingThrowData>.Request().Setup(new RulePerformSavingThrow(target.Entity, contextActionSavingThrow.Type, 0)))
					{
						actionsDamage = GetActionsDamage(ability, contextActionSavingThrow.Actions, context, casterPosition, target);
					}
					DamagePredictionData actionsDamage2;
					using (ContextData<SavingThrowData>.Request().Setup(new RulePerformSavingThrow(target.Entity, contextActionSavingThrow.Type, 100)))
					{
						actionsDamage2 = GetActionsDamage(ability, contextActionSavingThrow.Actions, context, casterPosition, target);
					}
					damagePredictionData2 = DamagePredictionData.Merge(actionsDamage, actionsDamage2);
				}
			}
			else if (gameAction is ContextActionConditionalSaved contextActionConditionalSaved)
			{
				if (context == null)
				{
					context = ability.ClaimExecutionContext(target, casterPosition);
				}
				using (context.SetScope(target))
				{
					DamagePredictionData actionsDamage3 = GetActionsDamage(ability, contextActionConditionalSaved.Failed, context, casterPosition, target);
					DamagePredictionData actionsDamage4 = GetActionsDamage(ability, contextActionConditionalSaved.Succeed, context, casterPosition, target);
					damagePredictionData2 = DamagePredictionData.Merge(actionsDamage3, actionsDamage4);
				}
			}
			else if (gameAction is ContextActionAttackWithFirstWeaponAbility contextActionAttackWithFirstWeaponAbility)
			{
				if (context == null)
				{
					context = ability.ClaimExecutionContext(target, casterPosition);
				}
				using (context.SetScope(target))
				{
					damagePredictionData2 = contextActionAttackWithFirstWeaponAbility.GetDamagePrediction(context, target.Entity, casterPosition);
				}
			}
			if (damagePredictionData2 != null)
			{
				if (damagePredictionData == null)
				{
					damagePredictionData = new DamagePredictionData();
				}
				damagePredictionData += damagePredictionData2;
			}
		}
		return damagePredictionData;
	}

	public static MoralePredictionRange GetMoralePrediction(this AbilityData ability, [CanBeNull] MechanicEntity target, Vector3 casterPosition, AbilityExecutionContext context = null)
	{
		if (target == null)
		{
			return default(MoralePredictionRange);
		}
		using (ProfileScope.New("AbilityDataHelper.GetMoralePrediction"))
		{
			using (ContextData<DisableStatefulRandomContext>.Request())
			{
				TargetWrapper target2 = target;
				if (context == null)
				{
					context = ability.ClaimExecutionContext(target2, casterPosition);
				}
				MoralePredictionRange abilityMoralePrediction = GetAbilityMoralePrediction(ability, target2, casterPosition, context);
				MoralePredictionData casterFactsMorale = GetCasterFactsMorale(ability, casterPosition, context, target);
				int minDelta = abilityMoralePrediction.MinDelta + casterFactsMorale.MoraleDelta;
				int maxDelta = abilityMoralePrediction.MaxDelta + casterFactsMorale.MoraleDelta;
				MoralePredictionRange result = default(MoralePredictionRange);
				result.MinDelta = minDelta;
				result.MaxDelta = maxDelta;
				return result;
			}
		}
	}

	private static MoralePredictionRange GetAbilityMoralePrediction(AbilityData ability, TargetWrapper target, Vector3 casterPosition, [CanBeNull] AbilityExecutionContext context)
	{
		using (ProfileScope.New("AbilityDataHelper.GetAbilityMoralePrediction"))
		{
			MoralePredictionData moralePredictionData = default(MoralePredictionData);
			MoralePredictionData moralePredictionData2 = default(MoralePredictionData);
			MoralePredictionData moralePredictionData3 = default(MoralePredictionData);
			bool flag = ability.Caster.IsAlly(target.Entity);
			foreach (AbilityEffectRunAction component in ability.Blueprint.GetComponents<AbilityEffectRunAction>())
			{
				try
				{
					moralePredictionData += GetActionsMorale(ability, component.Actions, context, casterPosition, target);
					ActionList actions = (flag ? component.ActionsOnAlly : component.ActionsOnEnemy);
					moralePredictionData += GetActionsMorale(ability, actions, context, casterPosition, target);
					if (!flag)
					{
						(MoralePredictionData, MoralePredictionData) skillChecksMorale = GetSkillChecksMorale(ability, component.ActionsOnEnemy, context, casterPosition, target);
						moralePredictionData2 += skillChecksMorale.Item1;
						moralePredictionData3 += skillChecksMorale.Item2;
					}
				}
				catch (Exception ex)
				{
					LogChannel.Default.Error(ex);
				}
			}
			int minDelta = moralePredictionData.MoraleDelta + moralePredictionData2.MoraleDelta;
			int maxDelta = moralePredictionData.MoraleDelta + moralePredictionData3.MoraleDelta;
			MoralePredictionRange result = default(MoralePredictionRange);
			result.MinDelta = minDelta;
			result.MaxDelta = maxDelta;
			return result;
		}
	}

	private static MoralePredictionData GetActionsMorale([NotNull] AbilityData ability, [NotNull] ActionList actions, [CanBeNull] AbilityExecutionContext context, Vector3 casterPosition, [NotNull] TargetWrapper target)
	{
		using (ProfileScope.New("AbilityDataHelper.GetActionsMorale"))
		{
			MoralePredictionData result = default(MoralePredictionData);
			GameAction[] actions2 = actions.Actions;
			for (int i = 0; i < actions2.Length; i++)
			{
				if (actions2[i] is ContextActionMoraleChange contextActionMoraleChange)
				{
					if (context == null)
					{
						context = ability.ClaimExecutionContext(target, casterPosition);
					}
					using (context.SetScope(target))
					{
						result += contextActionMoraleChange.GetMoralePrediction(context);
					}
				}
			}
			return result;
		}
	}

	private static (MoralePredictionData success, MoralePredictionData fail) GetSkillChecksMorale([NotNull] AbilityData ability, [NotNull] ActionList actions, [CanBeNull] AbilityExecutionContext context, Vector3 casterPosition, [NotNull] TargetWrapper target)
	{
		using (ProfileScope.New("AbilityDataHelper.GetSkillChecksMorale"))
		{
			MoralePredictionData item = default(MoralePredictionData);
			MoralePredictionData item2 = default(MoralePredictionData);
			GameAction[] actions2 = actions.Actions;
			for (int i = 0; i < actions2.Length; i++)
			{
				if (actions2[i] is ContextActionSkillCheck contextActionSkillCheck)
				{
					item += GetActionsMorale(ability, contextActionSkillCheck.Success, context, casterPosition, target);
					item2 += GetActionsMorale(ability, contextActionSkillCheck.Fail, context, casterPosition, target);
				}
			}
			return (success: item, fail: item2);
		}
	}

	private static MoralePredictionData GetCasterFactsMorale([NotNull] AbilityData ability, Vector3 casterPosition, AbilityExecutionContext context, [NotNull] TargetWrapper target)
	{
		using (ProfileScope.New("AbilityDataHelper.GetCasterFactsMorale"))
		{
			MoralePredictionData result = default(MoralePredictionData);
			MechanicEntity caster = ability.Caster;
			foreach (EntityFact item in caster.Facts.List)
			{
				foreach (AbilityTrigger component in item.Blueprint.GetComponents<AbilityTrigger>())
				{
					if (component.RestrictionsIsPassed(context, caster, target.Entity) && component.AbilityRestrictionsIsPassed(ability.Blueprint))
					{
						result += GetActionsMorale(ability, component.Actions, context, casterPosition, target);
					}
				}
			}
			return result;
		}
	}

	public static bool CanCastAoeAtPointerPositionFromDesiredPosition(this AbilityData ability, TargetWrapper target, Vector3 pointerPosition)
	{
		MechanicEntity caster = ability.Caster;
		Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(caster);
		return CanCastAoeAtPointerPosition(ability, target, desiredPosition, pointerPosition);
	}

	private static bool CanCastAoeAtPointerPosition(AbilityData ability, TargetWrapper target, Vector3 casterPosition, Vector3 pointerPosition)
	{
		AoEPattern aoEPattern = ability.Blueprint?.PatternSettings?.Pattern;
		if (aoEPattern == null)
		{
			PFLog.Ability.Error(ability.Name + " is expected to have a pattern but it doesn't");
			return true;
		}
		if (NeedCheckPattern(aoEPattern))
		{
			return IsWorldPositionInPattern(ability, target, casterPosition, pointerPosition);
		}
		return true;
		static bool NeedCheckPattern(AoEPattern aoePattern)
		{
			PatternType type = aoePattern.Type;
			if (type == PatternType.Circle || type == PatternType.Custom)
			{
				return true;
			}
			return false;
		}
	}

	private static bool IsWorldPositionInPattern(AbilityData abilityData, TargetWrapper target, Vector3 casterPosition, Vector3 worldPosition)
	{
		OrientedPatternData pattern = abilityData.GetPattern(target, casterPosition);
		GridNodeBase nearestNodeXZ = worldPosition.GetNearestNodeXZ();
		foreach (GridNodeBase node in pattern.Nodes)
		{
			if (node == nearestNodeXZ)
			{
				return true;
			}
		}
		return false;
	}

	public static ItemEntityWeapon GetWeaponForCharge(this AbilityData ability, MechanicEntity target)
	{
		if (!(ability.Blueprint.PatternSettings is AbilityCustomDirectMovement abilityCustomDirectMovement))
		{
			return null;
		}
		try
		{
			return GetActionsCharge(ability, null, abilityCustomDirectMovement.ActionsOnEncounteredTarget, target);
		}
		catch (Exception ex)
		{
			LogChannel.Default.Error(ex);
		}
		return null;
	}

	private static ItemEntityWeapon GetActionsCharge(AbilityData ability, [CanBeNull] AbilityExecutionContext context, ActionList actions, MechanicEntity target)
	{
		ItemEntityWeapon result = null;
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction action in actions2)
		{
			if (context == null)
			{
				context = ability.ClaimExecutionContext(target, ability.Caster.Position);
			}
			ItemEntityWeapon itemEntityWeapon = null;
			itemEntityWeapon = TryGetWeaponForChargeFromAction(ability, context, target, action);
			if (itemEntityWeapon != null)
			{
				result = itemEntityWeapon;
				break;
			}
		}
		return result;
	}

	private static ItemEntityWeapon TryGetWeaponForChargeFromAction(AbilityData ability, AbilityExecutionContext context, MechanicEntity target, GameAction action)
	{
		if (action is Conditional conditional)
		{
			bool flag = false;
			using (context.SetScope(target.ToITargetWrapper()))
			{
				flag = conditional.ConditionsChecker.Check();
			}
			if (!flag)
			{
				return GetActionsCharge(ability, context, conditional.IfFalse, target);
			}
			return GetActionsCharge(ability, context, conditional.IfTrue, target);
		}
		if (action is ContextActionAttackWithFirstWeaponAbility)
		{
			return ability.Caster.GetFirstWeapon();
		}
		if (action is ContextActionPerformAttack { UseCurrentWeapon: not false })
		{
			return ability.Caster.GetFirstWeapon();
		}
		return null;
	}

	public static void GatherAffectedTargetsData(this AbilityData ability, OrientedPatternData pattern, Vector3 casterPosition, TargetWrapper clickedTarget, in List<AbilityTargetUIData> listToFill)
	{
		if (!(clickedTarget == null))
		{
			if (ability.IsBurst)
			{
				GatherBurstPatternTargets(ability, casterPosition, clickedTarget, in listToFill);
			}
			else if (ability.IsAoe)
			{
				GatherAoEPatternTargets(ability, pattern, casterPosition, clickedTarget, in listToFill);
			}
		}
	}

	private static void GatherAoEPatternTargets(AbilityData ability, OrientedPatternData pattern, Vector3 casterPosition, TargetWrapper clickedTarget, in List<AbilityTargetUIData> listToFill)
	{
		using (ProfileScope.New("GatherAoEPatternTargets"))
		{
			AbilityExecutionContext abilityExecutionContext = ability.ClaimExecutionContext(clickedTarget);
			foreach (GridNodeBase node in pattern.Nodes)
			{
				foreach (MechanicEntity entity in node.GetEntities())
				{
					if (ability.IsValidTargetForAttack(entity))
					{
						AbilityTargetUIData? abilityTargetUIData = ObjectExtensions.Or(AbilityTargetUIDataCache.Instance, null)?.GetOrCreate(ability, casterPosition, entity, ability.PreciseBodyPart, abilityExecutionContext.ClickedTarget.Entity, null);
						if (abilityTargetUIData.HasValue)
						{
							listToFill.Add(abilityTargetUIData.Value);
						}
					}
				}
			}
		}
	}

	private static void GatherBurstPatternTargets(AbilityData ability, Vector3 casterPosition, TargetWrapper clickedTarget, in List<AbilityTargetUIData> listToFill)
	{
		using (ProfileScope.New("GatherBurstPatternTargets"))
		{
			AbilityExecutionContext abilityExecutionContext = ability.ClaimExecutionContext(clickedTarget);
			IReadOnlyList<MechanicEntity> burstPatternTargets = abilityExecutionContext.GetBurstPatternTargets(clickedTarget);
			if (burstPatternTargets == null)
			{
				return;
			}
			foreach (MechanicEntity item in burstPatternTargets)
			{
				if (ability.IsValidTargetForAttack(item))
				{
					AbilityTargetUIData? abilityTargetUIData = ObjectExtensions.Or(AbilityTargetUIDataCache.Instance, null)?.GetOrCreate(ability, casterPosition, item, ability.PreciseBodyPart, abilityExecutionContext.ClickedTarget.Entity, burstPatternTargets);
					if (abilityTargetUIData.HasValue)
					{
						listToFill.Add(abilityTargetUIData.Value);
					}
				}
			}
		}
	}

	private static IReadOnlyList<MechanicEntity> GetBurstPatternTargets(this AbilityExecutionContext context, TargetWrapper clickedTarget)
	{
		AbilityData ability = context.Ability;
		if (!ability.IsBurst || clickedTarget == null)
		{
			return null;
		}
		AbilityProjectileAttack abilityProjectileAttack;
		using (ProfileScope.New("Create Burst for Predict"))
		{
			abilityProjectileAttack = (ability.CanTargetPoint ? AbilityProjectileAttack.CreatePatternBurst(context, clickedTarget, ability.BurstAttacksCount, ability.Blueprint.IsControlledBurst) : AbilityProjectileAttack.CreateBurst(context, clickedTarget, ability.BurstAttacksCount, ability.Blueprint.IsControlledBurst));
		}
		return abilityProjectileAttack?.UnitsInPattern;
	}

	private static bool IsEntityAffected(AbilityExecutionContext context, MechanicEntity entity, OrientedPatternData pattern, Vector3? desiredPosition = null)
	{
		if (!context.Ability.IsValidTargetForAttack(entity))
		{
			return false;
		}
		if (entity is AbstractUnitEntity unit && IsUnitAffected(unit, pattern, desiredPosition))
		{
			return true;
		}
		if (entity is DestructibleEntity destructible && IsDestructibleAffected(destructible, pattern, desiredPosition))
		{
			return true;
		}
		return context.GetAdditionalTargets().Contains(entity);
	}

	private static bool IsUnitAffected(AbstractUnitEntity unit, OrientedPatternData pattern, Vector3? desiredPosition)
	{
		return IsAnyOccupiedNodeAffected(unit, pattern, desiredPosition);
	}

	private static bool IsDestructibleAffected(DestructibleEntity destructible, OrientedPatternData pattern, Vector3? desiredPosition)
	{
		PartCover optional = destructible.Parts.GetOptional<PartCover>();
		if (optional == null)
		{
			return IsAnyOccupiedNodeAffected(destructible, pattern, desiredPosition);
		}
		return IsCoverAffected(optional, pattern);
	}

	private static bool IsCoverAffected([NotNull] PartCover partCover, OrientedPatternData pattern)
	{
		GridObstacleCache instance = GridObstacleCache.Instance;
		if (instance == null)
		{
			return false;
		}
		foreach (GridObstacle gridObstacle in partCover.Owner.View.GridObstacles)
		{
			foreach (GridConnectionIndex affectedConnection in instance.GetAffectedConnections(gridObstacle))
			{
				if (pattern.Contains(affectedConnection.from) && pattern.Contains(affectedConnection.to))
				{
					return true;
				}
			}
		}
		return false;
	}

	private static bool IsAnyOccupiedNodeAffected(MechanicEntity entity, OrientedPatternData pattern, Vector3? desiredPosition)
	{
		return (desiredPosition.HasValue ? entity.GetOccupiedNodes(desiredPosition.Value) : entity.GetOccupiedNodes()).Any(((OrientedPatternData)pattern).Contains);
	}

	private static IEnumerable<MechanicEntity> GetAdditionalTargets(this AbilityExecutionContext context)
	{
		using (context.SetScope())
		{
			return ((context.Ability.Blueprint.GetComponent<AbilityEffectRunAction>()?.Actions.Actions.OfType<ContextActionOnOathOfVengeanceEnemies>().FirstOrDefault())?.GetTargets()).EmptyIfNull();
		}
	}

	private static bool CheckAffectedEntity(AbilityExecutionContext context, OrientedPatternData pattern, Vector3 casterPosition, MechanicEntity entity)
	{
		using (ProfileScope.New("CheckAffectedEntity"))
		{
			AbilityData ability = context.Ability;
			if (!IsEntityAffected(context, entity, pattern, (ability.Caster == entity) ? new Vector3?(casterPosition) : null))
			{
				return false;
			}
			if (ability.IsAoe || ability.IsCharge || ability.IsSingleTarget || ability.IsBurst)
			{
				return true;
			}
			return false;
		}
	}
}
