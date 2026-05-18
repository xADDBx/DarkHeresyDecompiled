using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
using Kingmaker.Utility;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility.EditorAttributes;

namespace Kingmaker.Code.Gameplay.Components.SkillCheck;

[Serializable]
[ComponentName("SkillCheck/SkillCheckRestrictionCalculator")]
public sealed class SkillCheckRestrictionCalculator : RestrictionCalculator
{
	public enum SkillSelectorType
	{
		Single,
		Combat,
		NonCombat,
		AttributeBased,
		All
	}

	private struct RuleInfo
	{
		public readonly MechanicEntity Initiator;

		public readonly StatType StatType;

		public readonly SkillCheckType Type;

		public readonly bool IsSaveFromMaxCritStage;

		public RuleInfo(RuleCalculateSkillCheck rule)
		{
			Initiator = rule.Initiator;
			StatType = rule.StatType;
			Type = rule.Type;
			IsSaveFromMaxCritStage = rule.IsSaveFromMaxCritStage;
		}
	}

	[EnumFlagsAsButtons]
	public SkillCheckTypeFlags TypeFilter = SkillCheckTypeFlags.Default;

	[InfoBox("Проверяет, что это сейв против последней (обычно 3й) стадии крита")]
	[ShowIf("IsStrictlyCritSave")]
	public bool FilterMaxCritStage;

	public SkillSelectorType SkillSelector;

	[ShowIf("IsSingleSkill")]
	public SkillType Skill;

	[ShowIf("IsAttributeBased")]
	public AttributeType Attribute;

	public bool AdvancedSkillOnly;

	private bool IsSingleSkill => SkillSelector == SkillSelectorType.Single;

	private bool IsAttributeBased => SkillSelector == SkillSelectorType.AttributeBased;

	private bool IsStrictlyCritSave => TypeFilter == SkillCheckTypeFlags.CritSave;

	protected override bool IsPassedInternal(MechanicEntity entity, IEvalContext context = null, TargetWrapper target = null, RulebookEvent rule = null, AbilityData ability = null)
	{
		if (!TryGetInfo(rule, out var info))
		{
			return false;
		}
		StatType statType = info.StatType;
		if (!statType.IsSkill())
		{
			return false;
		}
		if (!TypeFilter.HasAnyFlag(info.Type))
		{
			return false;
		}
		if (IsStrictlyCritSave && FilterMaxCritStage && !info.IsSaveFromMaxCritStage)
		{
			return false;
		}
		SkillType skillType = statType.ToSkillType();
		if (SkillSelector switch
		{
			SkillSelectorType.Single => (skillType == Skill) ? 1 : 0, 
			SkillSelectorType.Combat => skillType.IsCombatSkill() ? 1 : 0, 
			SkillSelectorType.NonCombat => (!skillType.IsCombatSkill()) ? 1 : 0, 
			SkillSelectorType.AttributeBased => (statType.GetBaseStat() == Attribute.ToStatType()) ? 1 : 0, 
			SkillSelectorType.All => 1, 
			_ => throw new ArgumentOutOfRangeException(), 
		} == 0)
		{
			return false;
		}
		if (AdvancedSkillOnly && !IsAdvancedSkill(info.Initiator, statType))
		{
			return false;
		}
		return base.IsPassedInternal(entity, context, target, rule, ability);
	}

	private bool TryGetInfo(RulebookEvent rule, out RuleInfo info)
	{
		if (rule is RulePerformSkillCheck rulePerformSkillCheck)
		{
			if (rulePerformSkillCheck.Initiator != null)
			{
				info = new RuleInfo(rulePerformSkillCheck.ChanceRule);
				return true;
			}
		}
		else if (rule is RuleCalculateSkillCheck { Initiator: not null } ruleCalculateSkillCheck)
		{
			info = new RuleInfo(ruleCalculateSkillCheck);
			return true;
		}
		info = default(RuleInfo);
		return false;
	}

	private bool IsAdvancedSkill(MechanicEntity entity, StatType skillType)
	{
		StatQueryOutput statQueryOutput = new StatQueryOutput();
		entity.Actor.GetStat(skillType, statQueryOutput, default(StatContext), "IsAdvancedSkill");
		foreach (Modifier modifier in statQueryOutput.Modifiers)
		{
			if (modifier.Fact?.Blueprint is BlueprintStatAdvancement)
			{
				return true;
			}
		}
		return false;
	}
}
