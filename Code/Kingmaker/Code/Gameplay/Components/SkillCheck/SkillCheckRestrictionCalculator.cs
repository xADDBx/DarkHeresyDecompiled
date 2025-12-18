using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Properties;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic.Progression.Features.Advancements;
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

	protected override bool IsPassedInternal(PropertyContext context)
	{
		if (!(context.Rule is RulePerformSkillCheck { Initiator: { } initiator, StatType: var statType, Type: var type } rulePerformSkillCheck))
		{
			return false;
		}
		if (!statType.IsSkill())
		{
			return false;
		}
		if (!TypeFilter.HasAnyFlag(type))
		{
			return false;
		}
		if (IsStrictlyCritSave && FilterMaxCritStage && !rulePerformSkillCheck.IsSaveFromMaxCritStage)
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
		if (AdvancedSkillOnly && !IsAdvancedSkill(initiator, statType))
		{
			return false;
		}
		return base.IsPassedInternal(context);
	}

	private bool IsAdvancedSkill(MechanicEntity entity, StatType skillType)
	{
		ModifiableValue statOptional = entity.GetStatOptional(skillType);
		if (statOptional == null)
		{
			return false;
		}
		foreach (Modifier modifier in statOptional.Modifiers)
		{
			if (modifier.Fact?.Blueprint is BlueprintStatAdvancement)
			{
				return true;
			}
		}
		return false;
	}
}
