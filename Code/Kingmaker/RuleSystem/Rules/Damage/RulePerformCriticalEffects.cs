using System;
using JetBrains.Annotations;
using Kingmaker.Code.Gameplay.Blueprints;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Utility.Damage;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Parts;

namespace Kingmaker.RuleSystem.Rules.Damage;

public sealed class RulePerformCriticalEffects : RulebookTargetEvent
{
	private readonly BlueprintBodyPart BodyPart;

	private readonly int Amount;

	[CanBeNull]
	public readonly RolledDamage Damage;

	public readonly FlagModifiersManager Immunity = new FlagModifiersManager();

	public bool DisableResistanceCheck { get; set; }

	public int ResultAmount { get; private set; }

	public RulePerformCriticalEffects(MechanicEntity initiator, MechanicEntity target, BlueprintBodyPart bodyPart, int amount)
		: base(initiator, target)
	{
		BodyPart = bodyPart;
		Amount = amount;
	}

	public RulePerformCriticalEffects(MechanicEntity initiator, MechanicEntity target, RolledDamage damage)
		: base(initiator, target)
	{
		BodyPart = damage.BodyPart;
		Amount = damage.ResultCritsCountValue;
		Damage = damage;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		PartHealth healthOptional = Target.GetHealthOptional();
		if (healthOptional == null)
		{
			return;
		}
		int criticalStage = healthOptional.GetCriticalStage(BodyPart);
		int num = Math.Min(BodyPart.CriticalEffectStagesCount - criticalStage, Amount);
		int num2 = 0;
		if ((bool)Immunity)
		{
			ResultAmount = 0;
			return;
		}
		if (DisableResistanceCheck)
		{
			num2 = num;
		}
		else
		{
			for (int i = 0; i < Amount; i++)
			{
				if (num2 >= num)
				{
					break;
				}
				RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(Target, StatType.SkillResistance, 0, base.Initiator, SkillCheckType.CritSave, criticalStage + num2 + 1 >= BodyPart.CriticalEffectStagesCount);
				Rulebook.Trigger(rulePerformSkillCheck);
				if (!rulePerformSkillCheck.ResultIsSuccess)
				{
					num2++;
				}
			}
		}
		ResultAmount = num2;
		healthOptional.AddCriticalEffectStages(BodyPart, ResultAmount, base.Initiator);
	}
}
