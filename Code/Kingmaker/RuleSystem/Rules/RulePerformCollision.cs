using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility;

namespace Kingmaker.RuleSystem.Rules;

[Serializable]
public class RulePerformCollision : RulebookTargetEvent<MechanicEntity, MechanicEntity>
{
	private const int PushMultiplier = 2;

	public readonly CompositeModifiersManager DamageModifiers = new CompositeModifiersManager(0);

	public int DamageRank { get; set; }

	public int ResultDamage { get; set; }

	public MechanicEntity Pushed => base.Target;

	public MechanicEntity Pusher => base.Initiator;

	public new NotImplementedException Initiator
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public new NotImplementedException Target
	{
		get
		{
			throw new NotImplementedException();
		}
	}

	public RulePerformCollision([NotNull] MechanicEntity pusher, MechanicEntity pushingEntity, int damageRank)
		: base(pusher, pushingEntity)
	{
		DamageRank = damageRank;
	}

	public override void OnTrigger(RulebookEventContext context)
	{
		RuleDealDamage preparedDealDamageRule = GetPreparedDealDamageRule();
		Rulebook.Trigger(preparedDealDamageRule);
		if (!(Pushed is UnitEntity))
		{
			return;
		}
		RulePerformSkillCheck rulePerformSkillCheck = new RulePerformSkillCheck(Pushed, StatType.Agility, 0);
		Rulebook.Trigger(rulePerformSkillCheck);
		if (!rulePerformSkillCheck.ResultIsSuccess)
		{
			BlueprintBuff proneCommonBuff = ConfigRoot.Instance.SystemMechanics.ProneCommonBuff;
			if (proneCommonBuff != null)
			{
				Pushed.Buffs.Add(proneCommonBuff, Pushed, null, 1.Rounds());
			}
		}
		ResultDamage = preparedDealDamageRule.ResultValue;
	}

	private RuleDealDamage GetPreparedDealDamageRule()
	{
		int value = DamageRank * 2;
		IntermediateDamage resultDamage = Rulebook.Trigger(new RuleCalculateDamage(Pusher, Pushed, null, null, new IntermediateDamage(DamageType.Impact, value))).ResultDamage;
		resultDamage.Modifiers.CopyFrom(DamageModifiers);
		return new RuleDealDamage(Pusher, Pushed, resultDamage)
		{
			IsCollisionDamage = true
		};
	}
}
