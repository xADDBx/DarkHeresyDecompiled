using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Controllers.Clicks;
using Kingmaker.Controllers.Clicks.Handlers;
using Kingmaker.Designers;
using Kingmaker.Designers.EventConditionActionSystem.Actions;
using Kingmaker.Designers.EventConditionActionSystem.ContextData;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Enums;
using Kingmaker.Framework;
using Kingmaker.Gameplay.Components;
using Kingmaker.Gameplay.ContextActions;
using Kingmaker.Gameplay.Features.Cohesion;
using Kingmaker.Gameplay.Features.Cohesion.Actions;
using Kingmaker.Gameplay.Parts;
using Kingmaker.Items;
using Kingmaker.Pathfinding;
using Kingmaker.Predictions;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Abilities.Components.Base;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Abilities.Components.ProjectileAttack;
using Kingmaker.UnitLogic.Commands.Base;
using Kingmaker.UnitLogic.FactLogic;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.StatefulRandom;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities;

public static class AbilityDataHelper
{
	private static List<MechanicEntity> m_CachedBurstTargets;

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

	public static DamagePredictionData GetDamagePrediction(this AbilityData ability, [CanBeNull] MechanicEntity target, Vector3 casterPosition, IEvalContext context = null)
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
				damagePrediction = ApplyActionEffects(damagePrediction, ability, target, casterPosition, context);
				return (damagePrediction.MaxDamage == 0) ? null : damagePrediction;
			}
		}
	}

	public static DamagePredictionData ApplyActionEffects(DamagePredictionData damagePrediction, AbilityData ability, [CanBeNull] MechanicEntity target, Vector3 casterPosition, IEvalContext context)
	{
		using (ProfileScope.New("AbilityDataHelper > AbilityEffectRunAction"))
		{
			bool flag = IsInCohesion(ability.Caster, target);
			TargetWrapper targetWrapper = null;
			foreach (AbilityEffectRunAction component in ability.Blueprint.GetComponents<AbilityEffectRunAction>())
			{
				try
				{
					if ((object)targetWrapper == null)
					{
						targetWrapper = target ?? Game.Instance.DefaultUnit;
					}
					DamagePredictionData actionsDamage = GetActionsDamage(ability, component.Actions, casterPosition, targetWrapper, context);
					damagePrediction += actionsDamage;
					ActionList actions = (ability.Caster.IsAlly(target) ? component.ActionsOnAlly : component.ActionsOnEnemy);
					DamagePredictionData actionsDamage2 = GetActionsDamage(ability, actions, casterPosition, targetWrapper, context);
					damagePrediction += actionsDamage2;
					if (flag)
					{
						DamagePredictionData casterCohesionDamage = GetCasterCohesionDamage(ability, component.Actions, casterPosition, targetWrapper, context);
						damagePrediction += casterCohesionDamage;
					}
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

	public static DamagePredictionData GetActionsDamage([NotNull] AbilityData ability, [NotNull] ActionList actions, Vector3 casterPosition, [NotNull] TargetWrapper target, [CanBeNull] IEvalContext context = null)
	{
		DamagePredictionData result = null;
		GameAction[] actions2 = actions.Actions;
		for (int i = 0; i < actions2.Length; i++)
		{
			DamagePredictionData actionDamage = GetActionDamage(actions2[i], ability, casterPosition, target, context);
			result += actionDamage;
		}
		return result;
	}

	private static DamagePredictionData GetActionDamage(GameAction action, [NotNull] AbilityData ability, Vector3 casterPosition, [NotNull] TargetWrapper target, [CanBeNull] IEvalContext context = null)
	{
		if (!(action is Conditional conditional))
		{
			if (!(action is ContextActionDealDamage contextActionDealDamage))
			{
				if (!(action is ContextActionSavingThrow contextActionSavingThrow))
				{
					if (!(action is ContextActionConditionalSaved contextActionConditionalSaved))
					{
						if (!(action is ContextActionSkillCheck contextActionSkillCheck))
						{
							if (!(action is ContextActionAttackWithFirstWeaponAbility contextActionAttackWithFirstWeaponAbility))
							{
								if (action is ContextActionCastSpell contextActionCastSpell)
								{
									AbilityData ability2 = new AbilityData(contextActionCastSpell.Spell, ability.Caster);
									return ApplyActionEffects(new DamagePredictionData(), ability2, target.Entity, casterPosition, null);
								}
								return null;
							}
							using ((context == null) ? EvalContext.PushAbility(ability, target, null, casterPosition).Get(out context) : default(EvalContext.StackFrameHandle))
							{
								return contextActionAttackWithFirstWeaponAbility.GetDamagePrediction(context, target.Entity, casterPosition);
							}
						}
						using ((context == null) ? EvalContext.PushAbility(ability, target, null, casterPosition).Get(out context) : default(EvalContext.StackFrameHandle))
						{
							DamagePredictionData actionsDamage = GetActionsDamage(ability, contextActionSkillCheck.Fail, casterPosition, target, context);
							DamagePredictionData actionsDamage2 = GetActionsDamage(ability, contextActionSkillCheck.Success, casterPosition, target, context);
							return DamagePredictionData.Merge(actionsDamage, actionsDamage2);
						}
					}
					using ((context == null) ? EvalContext.PushAbility(ability, target, null, casterPosition).Get(out context) : default(EvalContext.StackFrameHandle))
					{
						DamagePredictionData actionsDamage3 = GetActionsDamage(ability, contextActionConditionalSaved.Failed, casterPosition, target, context);
						DamagePredictionData actionsDamage4 = GetActionsDamage(ability, contextActionConditionalSaved.Succeed, casterPosition, target, context);
						return DamagePredictionData.Merge(actionsDamage3, actionsDamage4);
					}
				}
				using ((context == null) ? EvalContext.PushAbility(ability, target, null, casterPosition).Get(out context) : default(EvalContext.StackFrameHandle))
				{
					DamagePredictionData actionsDamage5;
					using (ContextData<SavingThrowData>.Request().Setup(new RulePerformSavingThrow(target.Entity, contextActionSavingThrow.Type, 0)))
					{
						actionsDamage5 = GetActionsDamage(ability, contextActionSavingThrow.Actions, casterPosition, target, context);
					}
					DamagePredictionData actionsDamage6;
					using (ContextData<SavingThrowData>.Request().Setup(new RulePerformSavingThrow(target.Entity, contextActionSavingThrow.Type, 100)))
					{
						actionsDamage6 = GetActionsDamage(ability, contextActionSavingThrow.Actions, casterPosition, target, context);
					}
					return DamagePredictionData.Merge(actionsDamage5, actionsDamage6);
				}
			}
			using ((context == null) ? EvalContext.PushAbility(ability, target, null, casterPosition).Get(out context) : default(EvalContext.StackFrameHandle))
			{
				return contextActionDealDamage.GetDamagePrediction(context, target);
			}
		}
		using ((context == null) ? EvalContext.PushAbility(ability, target, null, casterPosition).Get(out context) : default(EvalContext.StackFrameHandle))
		{
			ActionList conditionalActions = GetConditionalActions(conditional, target.Entity, context);
			return GetActionsDamage(ability, conditionalActions, casterPosition, target, context);
		}
	}

	private static DamagePredictionData GetCasterCohesionDamage([NotNull] AbilityData ability, [NotNull] ActionList actions, Vector3 casterPosition, [NotNull] TargetWrapper target, [CanBeNull] IEvalContext context = null)
	{
		using (ProfileScope.New("AbilityDataHelper > GetCasterCohesionDamage"))
		{
			DamagePredictionData damagePredictionData = null;
			GameAction[] actions2 = actions.Actions;
			foreach (GameAction gameAction in actions2)
			{
				if (gameAction is Conditional conditional)
				{
					using ((context == null) ? EvalContext.PushAbility(ability, target, casterPosition).Get(out context) : default(EvalContext.StackFrameHandle))
					{
						ActionList conditionalActions = GetConditionalActions(conditional, ability.Caster, context);
						DamagePredictionData casterCohesionDamage = GetCasterCohesionDamage(ability, conditionalActions, casterPosition, target, context);
						if (!(casterCohesionDamage == null))
						{
							if ((object)damagePredictionData == null)
							{
								damagePredictionData = new DamagePredictionData();
							}
							damagePredictionData += casterCohesionDamage;
						}
					}
				}
				else
				{
					if (!(gameAction is ContextActionOnUnitsInCohesionRange { TargetStrategy: ContextActionOnUnitsInCohesionRange.TargetSelection.All } contextActionOnUnitsInCohesionRange))
					{
						continue;
					}
					bool flag;
					using ((context == null) ? EvalContext.PushAbility(ability, target, ability.Caster, casterPosition).Get(out context) : EvalContext.PushContext(context))
					{
						flag = contextActionOnUnitsInCohesionRange.IsRestrictionsPassed(target.Entity, context);
					}
					if (!flag)
					{
						continue;
					}
					DamagePredictionData actionsDamage = GetActionsDamage(ability, contextActionOnUnitsInCohesionRange.Actions, casterPosition, target, context);
					if (!(actionsDamage == null))
					{
						if ((object)damagePredictionData == null)
						{
							damagePredictionData = new DamagePredictionData();
						}
						damagePredictionData += actionsDamage;
					}
				}
			}
			return damagePredictionData;
		}
	}

	private static bool IsInCohesion(MechanicEntity cohesionOwner, MechanicEntity cohesionTarget)
	{
		return cohesionOwner.GetOptional<PartCohesion>()?.ContainsInRange(cohesionTarget) ?? false;
	}

	private static ActionList GetConditionalActions(Conditional conditional, MechanicEntity conditionalTarget, [NotNull] IEvalContext context)
	{
		bool flag;
		using (EvalContext.PushContext(context, conditionalTarget))
		{
			flag = conditional.ConditionsChecker.Check();
		}
		if (!flag)
		{
			return conditional.IfFalse;
		}
		return conditional.IfTrue;
	}

	public static int CalculateHitChanceWithAvoidance(float initialHitChance, float defenceChance, float coverChance, float overpenetraionChance)
	{
		float num = Mathf.Clamp(defenceChance, 0f, 100f);
		float num2 = Mathf.Clamp(coverChance, 0f, 100f);
		return Mathf.Min(UtilityMath.ToPercent(UtilityMath.ToFraction(initialHitChance) * UtilityMath.ToFraction(100f - num) * UtilityMath.ToFraction(100f - num2) * ((overpenetraionChance > 0f) ? UtilityMath.ToFraction(overpenetraionChance) : 1f)), 100);
	}

	public static int CalculateCoverHitChance(LosCalculations.CoverType los, MechanicEntity entity)
	{
		if (los != LosCalculations.CoverType.Cover)
		{
			return 0;
		}
		int num = 0;
		foreach (BlueprintBodyPart bodyPart in entity.BodyParts)
		{
			if (bodyPart.CanBeHitRandomly && bodyPart.ReplaceableByCover)
			{
				num += bodyPart.HitChance;
			}
		}
		return Mathf.Min(num, 100);
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
				MoralePredictionRange targetChanceTriggers = GetTargetChanceTriggers(ability, casterPosition, context, target);
				int min = abilityMoralePrediction.MinDelta + targetChanceTriggers.MinDelta + casterFactsMorale.MoraleDelta;
				int max = abilityMoralePrediction.MaxDelta + targetChanceTriggers.MaxDelta + casterFactsMorale.MoraleDelta;
				(int minDelta, int maxDelta) tuple = ClampMoraleDelta(target, ability.Caster, min, max);
				int item = tuple.minDelta;
				int item2 = tuple.maxDelta;
				MoralePredictionRange result = default(MoralePredictionRange);
				result.MinDelta = item;
				result.MaxDelta = item2;
				return result;
			}
		}
	}

	private static (int minDelta, int maxDelta) ClampMoraleDelta(MechanicEntity target, MechanicEntity caster, int min, int max)
	{
		int num = min;
		int num2 = max;
		(int lowerLimit, int upperLimit) targetMoraleLimits = GetTargetMoraleLimits(target, caster);
		int item = targetMoraleLimits.lowerLimit;
		int item2 = targetMoraleLimits.upperLimit;
		num = Mathf.Clamp(min, item, item2);
		num2 = Mathf.Clamp(max, item, item2);
		if (!(target is BaseUnitEntity { Morale: var morale }))
		{
			return (minDelta: num, maxDelta: num2);
		}
		if (morale == null)
		{
			return (minDelta: num, maxDelta: num2);
		}
		int value = morale.Value;
		num = LimitDelta(num, value, item, item2);
		num2 = LimitDelta(num2, value, item, item2);
		return (minDelta: num, maxDelta: num2);
		static int LimitDelta(int delta, int current, int lower, int upper)
		{
			int num3 = current + delta;
			if (num3 > upper)
			{
				return upper - current;
			}
			if (num3 < lower)
			{
				return lower - current;
			}
			return delta;
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
			int num = Mathf.Min(moralePredictionData2.MoraleDelta, moralePredictionData3.MoraleDelta);
			int num2 = Mathf.Max(moralePredictionData2.MoraleDelta, moralePredictionData3.MoraleDelta);
			int minDelta = moralePredictionData.MoraleDelta + num;
			int maxDelta = moralePredictionData.MoraleDelta + num2;
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
					result += contextActionMoraleChange.GetMoralePrediction(context);
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
					if (component.RestrictionsIsPassed(EvalContext.Current, caster, target.Entity) && component.AbilityRestrictionsIsPassed(ability.Blueprint))
					{
						result += GetActionsMorale(ability, component.Actions, context, casterPosition, target);
					}
				}
			}
			return result;
		}
	}

	private static (int lowerLimit, int upperLimit) GetTargetMoraleLimits(MechanicEntity target, MechanicEntity caster)
	{
		MoraleRoot instance = MoraleRoot.Instance;
		CompositeModifiersManager compositeModifiersManager = new CompositeModifiersManager();
		CompositeModifiersManager compositeModifiersManager2 = new CompositeModifiersManager();
		compositeModifiersManager.Add(ModifierType.ValAdd, instance.MinValue);
		compositeModifiersManager2.Add(ModifierType.ValAdd, instance.MaxValue);
		RuleCalculateMoraleChange ruleCalculateMoraleChange = null;
		foreach (EntityFact item in target.Facts.List)
		{
			if (!(item is MechanicEntityFact sourceFact))
			{
				continue;
			}
			foreach (MoraleChangeModifierTarget component in item.Blueprint.GetComponents<MoraleChangeModifierTarget>())
			{
				if (ruleCalculateMoraleChange == null)
				{
					ruleCalculateMoraleChange = new RuleCalculateMoraleChange(caster, target, MoraleEventType.ForcedChangeMoralePhase);
				}
				if (component.IsRestrictionPassed(EvalContext.Current, ruleCalculateMoraleChange))
				{
					component.BottomLimitModifier.TryApply(compositeModifiersManager, sourceFact, component.Descriptor);
					component.TopLimitModifier.TryApply(compositeModifiersManager2, sourceFact, component.Descriptor);
				}
			}
		}
		return (lowerLimit: compositeModifiersManager.Value, upperLimit: compositeModifiersManager2.Value);
	}

	private static MoralePredictionRange GetTargetChanceTriggers([NotNull] AbilityData ability, Vector3 casterPosition, AbilityExecutionContext context, [NotNull] TargetWrapper target)
	{
		if (!AbilityRollsChance(ability) || target.Entity == null)
		{
			return default(MoralePredictionRange);
		}
		int num = 0;
		int num2 = 0;
		foreach (EntityFact item in target.Entity.Facts.List)
		{
			foreach (ChanceRollTrigger component in item.Blueprint.GetComponents<ChanceRollTrigger>())
			{
				try
				{
					if (component.Restrictions.IsPassed(EvalContext.Current, null, target, null, ability))
					{
						int moraleDelta = GetActionsMorale(ability, component.OnAnyResult, context, casterPosition, target).MoraleDelta;
						int moraleDelta2 = GetActionsMorale(ability, component.OnSuccess, context, casterPosition, target).MoraleDelta;
						int moraleDelta3 = GetActionsMorale(ability, component.OnFailure, context, casterPosition, target).MoraleDelta;
						num += moraleDelta + Mathf.Min(moraleDelta2, moraleDelta3);
						num2 += moraleDelta + Mathf.Max(moraleDelta2, moraleDelta3);
					}
				}
				catch (Exception ex)
				{
					PFLog.Default.Exception(ex);
				}
			}
		}
		MoralePredictionRange result = default(MoralePredictionRange);
		result.MinDelta = num;
		result.MaxDelta = num2;
		return result;
	}

	private static bool AbilityRollsChance([NotNull] AbilityData ability)
	{
		foreach (AbilityEffectRunAction component in ability.Blueprint.GetComponents<AbilityEffectRunAction>())
		{
			if (ActionListRollsChance(component.Actions) || ActionListRollsChance(component.ActionsOnAlly) || ActionListRollsChance(component.ActionsOnEnemy))
			{
				return true;
			}
		}
		return false;
	}

	private static bool ActionListRollsChance([CanBeNull] ActionList actions)
	{
		if (actions?.Actions == null)
		{
			return false;
		}
		GameAction[] actions2 = actions.Actions;
		foreach (GameAction gameAction in actions2)
		{
			if (gameAction is ContextActionPerformAttack || gameAction is ContextActionSkillCheck)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsPatternRestrictionPassed(this AbilityData abilityData, TargetWrapper target)
	{
		return abilityData.Blueprint?.GetComponent<IAbilityPatternRestriction>()?.IsPatternRestrictionPassed(abilityData, abilityData.Caster, target) ?? true;
	}

	public static bool IsAoEPatternEmpty(this AbilityData ability, Vector3 casterPosition, TargetWrapper target)
	{
		if (ability.Blueprint?.PatternSettings?.Pattern == null)
		{
			return false;
		}
		IAbilityAoEPatternProvider patternSettings = ability.GetPatternSettings();
		GridNodeBase bestShootingPosition = ability.GetBestShootingPosition(casterPosition.GetNearestNodeXZUnwalkable(), target);
		GridNodeBase actualCastNode = AoEPatternHelper.GetActualCastNode(ability.Caster, bestShootingPosition, target.Point, ability.MinRangeCells, ability.RangeCells);
		Size targetSizeForPattern = ability.GetTargetSizeForPattern(target);
		OrientedPatternData orientedPattern = patternSettings.GetOrientedPattern(ability, bestShootingPosition, actualCastNode, targetSizeForPattern);
		if (!orientedPattern.Equals(default(OrientedPatternData)))
		{
			return orientedPattern.Nodes.IsEmpty;
		}
		return false;
	}

	public static bool IsTargetInBurstPattern(this AbilityData ability, Vector3 casterPosition, TargetWrapper target)
	{
		if (!ability.IsBurst)
		{
			return false;
		}
		using (ProfileScope.New("AbilityDataHelper.IsTargetInBurstPattern"))
		{
			IReadOnlyList<MechanicEntity> burstPatternTargets = ability.ClaimExecutionContext(target).GetBurstPatternTargets(casterPosition.GetNearestNodeXZUnwalkable(), target);
			if (burstPatternTargets == null)
			{
				return false;
			}
			foreach (MechanicEntity item in burstPatternTargets)
			{
				if (item == target.Entity)
				{
					return true;
				}
			}
			return false;
		}
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
		if (ability == null)
		{
			return;
		}
		using (ProfileScope.New("GatherAoEPatternTargets"))
		{
			AbilityExecutionContext abilityExecutionContext = ability.ClaimExecutionContext(clickedTarget);
			foreach (GridNodeBase node in pattern.Nodes)
			{
				foreach (MechanicEntity entity in node.GetEntities())
				{
					if (ability.IsValidTargetForAttack(entity) && entity.IsVisibleForPlayer)
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
		if (ability == null)
		{
			return;
		}
		using (ProfileScope.New("GatherBurstPatternTargets"))
		{
			AbilityExecutionContext abilityExecutionContext = ability.ClaimExecutionContext(clickedTarget);
			IReadOnlyList<MechanicEntity> burstPatternTargets = abilityExecutionContext.GetBurstPatternTargets(casterPosition.GetNearestNodeXZUnwalkable(), clickedTarget);
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

	private static IReadOnlyList<MechanicEntity> GetBurstPatternTargets(this AbilityExecutionContext context, GridNodeBase casterNode, TargetWrapper clickedTarget)
	{
		AbilityData ability = context.Ability;
		if (!ability.IsBurst || clickedTarget == null)
		{
			return null;
		}
		m_CachedBurstTargets = null;
		using (ProfileScope.New("Predict burst targets"))
		{
			m_CachedBurstTargets = (ability.CanTargetPoint ? AbilityProjectileAttack.GetPatternBurstTargets(context, clickedTarget) : AbilityProjectileAttack.GetBurstTargets(context, casterNode, clickedTarget));
		}
		return m_CachedBurstTargets;
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
		if (ability.Caster.IsAlly(target))
		{
			if ((bool)ability.Caster.Features.AttacksIgnoreAllies)
			{
				return false;
			}
			PartAttackIgnoreAllies optional = ability.Caster.GetOptional<PartAttackIgnoreAllies>();
			if (optional != null && optional.ShouldIgnoreAllies(ability))
			{
				return false;
			}
		}
		if (!ability.Blueprint.CanTargetDestructibleObjects && target is DestructibleEntity)
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

	public static List<GridNodeBase> GetSingleShotAffectedNodes(this AbilityData ability, GridNodeBase casterNode, TargetWrapper target)
	{
		if (!ability.IsSingleTarget || target?.Entity == null)
		{
			return TempList.Get<GridNodeBase>();
		}
		return AbilityProjectileAttack.GetSingleShotAffectedNodes(ability, casterNode, target.Entity).Nodes;
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

	public static bool CanCastAbility(this AbilityData ability, TargetWrapper target, Vector3? pointerPosition, out AbilityData.UnavailabilityReasonType? reason)
	{
		using (ProfileScope.New("AbilityDataHelper.CanUseAbility"))
		{
			reason = AbilityData.UnavailabilityReasonType.None;
			if (ability == null)
			{
				reason = AbilityData.UnavailabilityReasonType.Unknown;
				return false;
			}
			if (target == null || (!ability.CanTargetPoint && target.IsPoint))
			{
				reason = AbilityData.UnavailabilityReasonType.NullTarget;
				return false;
			}
			if (ability.IsPrecise && !(target.Entity is BaseUnitEntity))
			{
				reason = AbilityData.UnavailabilityReasonType.TargetCannotBeAttackedByPreciseAttack;
				return true;
			}
			if (!ability.CanTargetFromDesiredPosition(target, out var unavailabilityReason))
			{
				reason = unavailabilityReason;
			}
			Vector3 pointerPosition2 = pointerPosition ?? target.Point;
			Vector3 desiredPosition = Game.Instance.Controllers.VirtualPositionController.GetDesiredPosition(ability.Caster);
			if (ability.IsAoe && ability.IsAoEPatternEmpty(desiredPosition, target))
			{
				reason = AbilityData.UnavailabilityReasonType.Unknown;
			}
			if (ability.IsAoe && !CanCastAoeAtPointerPosition(ability, target, desiredPosition, pointerPosition2))
			{
				reason = AbilityData.UnavailabilityReasonType.TargetTooFar;
			}
			if (ability.IsBurst && !ability.CanTargetPoint && !ability.IsTargetInBurstPattern(desiredPosition, target))
			{
				reason = AbilityData.UnavailabilityReasonType.Unknown;
			}
			if (!Game.Instance.Player.IsInCombat && reason == AbilityData.UnavailabilityReasonType.TargetTooFar)
			{
				return true;
			}
			return reason == AbilityData.UnavailabilityReasonType.None;
		}
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
		using (ContextData<DisableStatefulRandomContext>.Request())
		{
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
	}

	private static HealPredictionData TryGetHealPredictionFromAction(AbilityData ability, AbilityExecutionContext context, MechanicEntity target, GameAction action)
	{
		if (action is Conditional conditional)
		{
			bool flag = false;
			using (EvalContext.PushContext(context, target))
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

	public static bool CanCastAoeAtPointerPosition(AbilityData ability, TargetWrapper target, Vector3 casterPosition, Vector3 pointerPosition)
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
			using (EvalContext.PushContext(context, target))
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
}
