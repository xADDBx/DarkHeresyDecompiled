using System;
using System.Linq;
using System.Text;
using Code.Enums;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.RuleDOT;
using Kingmaker.UnitLogic.Buffs;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Actions;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Mechanics.Actions;

[Serializable]
[TypeId("984ce206d35c47f8adce3d3e47fc10ec")]
public class ContextActionApplyDOT : ContextAction
{
	public DOT Type;

	public ContextValue DamageValue;

	public BuffEndCondition EndCondition = BuffEndCondition.CombatEnd;

	public bool UseRoundsDuration;

	[ShowIf("UseRoundsDuration")]
	public Rounds RoundsDuration;

	public bool UseSavingThrowOverride;

	[ShowIf("UseSavingThrowOverride")]
	public SkillType SavingThrowOverride;

	public bool UseDifficultyOverride;

	[ShowIf("UseDifficultyOverride")]
	public ContextValue DifficultyOverride;

	public bool UsePenetrationOverride;

	[ShowIf("UsePenetrationOverride")]
	public ContextValue PenetrationOverride;

	protected override void RunAction()
	{
		if (!(base.Target.Entity is BaseUnitEntity baseUnitEntity))
		{
			return;
		}
		BlueprintBuff blueprintBuff = SelectBuff(Type);
		if (blueprintBuff != null && !TryRollSavingThrow())
		{
			int baseRank = DamageValue.Calculate(base.Context);
			int num = Math.Max(1, Rulebook.Trigger(new RuleCalculateDOT(base.Caster, baseUnitEntity, Type, baseRank)).ResultRank);
			Buff buff = base.Target.Entity.Buffs.GetBuff(blueprintBuff);
			if (buff == null)
			{
				Rounds? rounds = (UseRoundsDuration ? new Rounds?(RoundsDuration) : null);
				BuffDuration duration = new BuffDuration(rounds, EndCondition);
				baseUnitEntity.Buffs.Add(blueprintBuff, base.Caster, base.Context, duration, num)?.TryAddSource(this);
			}
			else
			{
				buff.TryAddSource(this);
				buff.AddRank(num, base.Caster);
			}
		}
	}

	private bool TryRollSavingThrow()
	{
		if (!DOTRoot.Instance.TryGetDOTBuffOfType(Type, out var buff))
		{
			return false;
		}
		DOTLogic dOTLogic = buff.GetComponents<DOTLogic>().First((DOTLogic c) => c.Type == Type);
		PropertyContext context = new PropertyContext(base.Caster, base.Context, base.Target);
		if (!dOTLogic.SaveThrowRestrictions.IsPassed(context))
		{
			return false;
		}
		if (dOTLogic.SaveType == SkillType.Unknown)
		{
			return false;
		}
		SkillType? skillType = (UseSavingThrowOverride ? new SkillType?(SavingThrowOverride) : null);
		int? num = (UseDifficultyOverride ? new int?(DifficultyOverride.Calculate(base.Context)) : null);
		RulePerformSkillCheck obj = new RulePerformSkillCheck(base.TargetEntity, skillType ?? dOTLogic.SaveType, num ?? dOTLogic.Difficulty, SkillCheckType.DOT)
		{
			Reason = base.TargetEntity
		};
		Rulebook.Trigger(obj);
		if (obj.ResultIsSuccess)
		{
			return true;
		}
		return false;
	}

	[CanBeNull]
	public static BlueprintBuff SelectBuff(DOT type)
	{
		DOTRoot.Instance.TryGetDOTBuffOfType(type, out var buff);
		return buff;
	}

	public override string GetCaption()
	{
		using PooledStringBuilder pooledStringBuilder = ContextData<PooledStringBuilder>.Request();
		StringBuilder builder = pooledStringBuilder.Builder;
		builder.Append("Apply DOT: ");
		builder.Append(DamageValue.ToString());
		builder.Append(" [");
		builder.Append(Type.ToString());
		builder.Append("]");
		if (UseRoundsDuration)
		{
			builder.Append(" for ");
			builder.Append(RoundsDuration.ToString());
			builder.Append(" rounds");
		}
		if (EndCondition == BuffEndCondition.CombatEnd)
		{
			builder.Append(" until ");
			builder.Append(EndCondition.ToString());
		}
		if (!UseRoundsDuration && EndCondition == BuffEndCondition.RemainAfterCombat)
		{
			builder.Append(" permanently");
		}
		return builder.ToString();
	}
}
