using JetBrains.Annotations;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Framework.Abilities;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.MapObjects;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.ContextActions;

[TypeId("892a1b239add4a50b1f09acdbf98a54e")]
public class ContextActionSkillCheck : ContextAction, IAbilityPrediction
{
	public SkillType Skill;

	[SkillCheckActualDifficulty]
	public SkillCheckDifficulty Difficulty;

	[ShowIf("IsDifficultyCustom")]
	public ContextValue CustomDifficulty;

	public ActionList Success;

	public ActionList Fail;

	[UsedImplicitly]
	private bool IsDifficultyCustom => Difficulty == SkillCheckDifficulty.Custom;

	public void CollectPrediction(AbilityPredictionContext context)
	{
		AbilityPredictionContext context = context;
		MechanicEntity mechanicEntity = base.Target?.Entity;
		if (mechanicEntity == null)
		{
			return;
		}
		MechanicEntity caster = base.Caster;
		float num = Mathf.Clamp01((float)GetSuccessChance(mechanicEntity, caster, base.Context) / 100f);
		if (num > 0f)
		{
			context.WithProbability(num, delegate
			{
				context.ProcessActionList(Success);
			});
		}
		if (num < 1f)
		{
			context.WithProbability(1f - num, delegate
			{
				context.ProcessActionList(Fail);
			});
		}
	}

	public override string GetCaption()
	{
		return string.Format("Check {0} with difficulty {1}{2}", Skill, Difficulty, (Difficulty == SkillCheckDifficulty.Custom) ? $"{CustomDifficulty}" : "");
	}

	public int GetSuccessChance(MechanicEntity target, MechanicEntity caster, IEvalContext context)
	{
		int difficulty = GetDifficulty(context);
		StatType statType = Skill.ToStatType();
		return Rulebook.Trigger(new RuleCalculateSkillCheck(target, statType, difficulty, SkillCheckType.Default, caster)).ResultChance;
	}

	protected override void RunAction()
	{
		MechanicEntity entity = base.Target.Entity;
		if (entity != null)
		{
			MechanicEntity attacker = base.Caster ?? entity;
			int difficulty = GetDifficulty(base.Context);
			if (Rulebook.Trigger(new RulePerformSkillCheck(entity, Skill.ToStatType(), difficulty, attacker)
			{
				Reason = new RuleReason(base.Context)
			}).ResultIsSuccess)
			{
				Success.Run();
			}
			else
			{
				Fail.Run();
			}
		}
	}

	private int GetDifficulty(IEvalContext context)
	{
		if (Difficulty != 0)
		{
			return Difficulty.GetDC();
		}
		return CustomDifficulty.Calculate(context);
	}
}
