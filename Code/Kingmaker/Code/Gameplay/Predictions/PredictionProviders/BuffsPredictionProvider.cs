using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Gameplay.ContextActions;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.CodeTimer;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Predictions.PredictionProviders;

public class BuffsPredictionProvider : IPredictionProvider<UIBuffsPredictionData, UIPredictionContext>
{
	private static readonly string m_ProfilerScopeId = "BuffsPredictionProvider.Get()";

	public UIBuffsPredictionData Get(UIPredictionContext ctx)
	{
		using (ProfileScope.New(m_ProfilerScopeId))
		{
			if (!ctx.Ability.CanTargetFromDesiredPosition(ctx.Target))
			{
				return default(UIBuffsPredictionData);
			}
			using BuffsPredictionContext ctx2 = new BuffsPredictionContext(ctx);
			return Get(ctx2);
		}
	}

	public UIBuffsPredictionData Get(BuffsPredictionContext ctx)
	{
		foreach (AbilityEffectRunAction component in ctx.Ability.Blueprint.OriginalBlueprint.GetComponents<AbilityEffectRunAction>())
		{
			CollectBuffs(ctx, component.Actions);
			CollectBuffs(ctx, ctx.Target.IsPlayerEnemy ? component.ActionsOnEnemy : component.ActionsOnAlly);
		}
		UIBuffsPredictionData result = default(UIBuffsPredictionData);
		result.PredictedBuffs = ctx.PredictedBuffs;
		return result;
	}

	private void CollectBuffs(BuffsPredictionContext ctx, ActionList actionList)
	{
		GameAction[] actions = actionList.Actions;
		foreach (GameAction gameAction in actions)
		{
			if (!(gameAction is ContextActionSkillCheck skillCheck))
			{
				if (gameAction is ContextActionApplyBuff contextActionApplyBuff)
				{
					ctx.AddGuaranteedBuff(contextActionApplyBuff.Buff);
				}
			}
			else
			{
				ProcessSkillCheck(ctx, skillCheck);
			}
		}
	}

	private static void ProcessSkillCheck(BuffsPredictionContext ctx, ContextActionSkillCheck skillCheck)
	{
		GameAction[] actions = skillCheck.Fail.Actions;
		for (int i = 0; i < actions.Length; i++)
		{
			if (actions[i] is ContextActionApplyBuff { Buff: var buff })
			{
				int buffApplyChance = GetBuffApplyChance(ctx, skillCheck, buff);
				ctx.AddBuffWithChance(buff, buffApplyChance);
			}
		}
	}

	private static int GetBuffApplyChance(BuffsPredictionContext ctx, ContextActionSkillCheck skillCheck, BlueprintBuff buff)
	{
		if (ctx.IsTargetImmuneToBuff(buff))
		{
			return 0;
		}
		MechanicEntity target = ctx.Target;
		MechanicEntity caster = ctx.Ability.Caster;
		AbilityExecutionContext abilityExecutionContext = ctx.AbilityExecutionContext;
		int successChance = skillCheck.GetSuccessChance(target, caster, abilityExecutionContext);
		return 100 - Mathf.Clamp(successChance, 0, 100);
	}
}
