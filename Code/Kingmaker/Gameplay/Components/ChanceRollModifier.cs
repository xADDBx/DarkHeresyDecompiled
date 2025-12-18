using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.RuleSystem.Rules.Interfaces;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.EditorAttributes;
using UnityEngine;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[TypeId("5b8af533b85d449f80099a0d08686165")]
public abstract class ChanceRollModifier : MechanicEntityFactComponentDelegate
{
	public enum ModifierType
	{
		ForceResult,
		Reroll,
		Modify
	}

	public enum RerollType
	{
		Failure,
		Success
	}

	public enum StatFilterType
	{
		None,
		Attribute,
		Skill
	}

	public enum SuccessChanceOverrideType
	{
		KeepOriginal,
		ReplaceWithModifier,
		AddModifier
	}

	private sealed class ComponentData : IEntityFactComponentTransientData
	{
		public RuleRollChance Rule;
	}

	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	[EnumFlagsAsButtons]
	public ChanceRollType TypeFilter;

	public StatFilterType StatFilter;

	[ShowIf("IsAttributeFilter")]
	public AttributeType AttributeFilter;

	[ShowIf("IsSkillFilter")]
	public SkillType SkillFilter;

	public ModifierType Type;

	[ShowIf("IsForceResult")]
	public ContextValue ForceResult = new ContextValue
	{
		Value = 1
	};

	[ShowIf("IsReroll")]
	public RerollType Reroll;

	[ShowIf("IsReroll")]
	public ContextValue RerollCount = new ContextValue
	{
		Value = 1
	};

	[ShowIf("IsReroll")]
	public SuccessChanceOverrideType SuccessChanceOverride;

	[Tooltip("Модифицированный шанс на успех может превышать оригинальный")]
	[ShowIf("IsReplaceChanceWithModifier")]
	public bool AllowModifiedChanceToExceedOriginal;

	[ShowIf("ShowSuccessChanceModifier")]
	public ContextValue SuccessChanceModifier = new ContextValue
	{
		Value = 50
	};

	[ShowIf("IsModify")]
	[InfoBox("Модифицирует результат броска кубика")]
	public ContextValueModifierWithType Modifier = new ContextValueModifierWithType
	{
		Enabled = true
	};

	public ActionList OnAnyResult;

	public ActionList OnSuccess;

	public ActionList OnFailure;

	private bool IsReroll => Type == ModifierType.Reroll;

	private bool IsForceResult => Type == ModifierType.ForceResult;

	private bool IsModify => Type == ModifierType.Modify;

	private bool IsAttributeFilter => StatFilter == StatFilterType.Attribute;

	private bool IsSkillFilter => StatFilter == StatFilterType.Skill;

	private bool ShowSuccessChanceModifier
	{
		get
		{
			if (IsReroll)
			{
				return SuccessChanceOverride != SuccessChanceOverrideType.KeepOriginal;
			}
			return false;
		}
	}

	private bool IsReplaceChanceWithModifier
	{
		get
		{
			if (ShowSuccessChanceModifier)
			{
				return SuccessChanceOverride == SuccessChanceOverrideType.ReplaceWithModifier;
			}
			return false;
		}
	}

	protected void TryApplyBefore(RuleRollChance rule)
	{
		if (!IsSuitable(rule))
		{
			return;
		}
		RequestTransientData<ComponentData>().Rule = rule;
		switch (Type)
		{
		case ModifierType.Reroll:
		{
			int num = RerollCount.Calculate(base.Context);
			int rerollSuccessChance = GetRerollSuccessChance(rule);
			switch (Reroll)
			{
			case RerollType.Failure:
				rule.AddRerollFail(rerollSuccessChance, num, base.Fact);
				break;
			case RerollType.Success:
				rule.AddRerollSuccess(rerollSuccessChance, num, base.Fact);
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
			break;
		}
		case ModifierType.ForceResult:
			rule.Override(ForceResult.Calculate(base.Context), base.Fact);
			break;
		case ModifierType.Modify:
			Modifier.TryApply(rule.Modifiers, base.Fact, ModifierDescriptor.None);
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
	}

	protected void TryApplyAfter(RuleRollChance rule)
	{
		ComponentData componentData = RequestTransientData<ComponentData>();
		if (componentData.Rule != rule)
		{
			return;
		}
		componentData.Rule = null;
		if (rule.Rerolls.Contains((RerollData i) => i.Source == base.Fact))
		{
			OnAnyResult.Run();
			if (rule.Success)
			{
				OnSuccess.Run();
			}
			else
			{
				OnFailure.Run();
			}
		}
	}

	private bool IsSuitable(RuleRollChance rule)
	{
		if (!TypeFilter.HasAnyFlag(rule.Type))
		{
			return false;
		}
		if (AttributeFilter != 0 && rule.AttributeType != AttributeFilter && rule.SkillType?.GetBaseAttribute() != AttributeFilter)
		{
			return false;
		}
		if (SkillFilter != 0 && rule.SkillType != SkillFilter)
		{
			return false;
		}
		if (!Restrictions.IsPassed(base.Context, base.Owner, rule.AttackInitiator, rule))
		{
			return false;
		}
		return true;
	}

	private int GetRerollSuccessChance(RuleRollChance rule)
	{
		if (!IsReroll)
		{
			throw new InvalidOperationException();
		}
		return SuccessChanceOverride switch
		{
			SuccessChanceOverrideType.KeepOriginal => rule.OriginalSuccessChance, 
			SuccessChanceOverrideType.ReplaceWithModifier => AllowModifiedChanceToExceedOriginal ? SuccessChanceModifier.Calculate(base.Context) : Math.Min(rule.OriginalSuccessChance, SuccessChanceModifier.Calculate(base.Context)), 
			SuccessChanceOverrideType.AddModifier => Math.Clamp(rule.OriginalSuccessChance + SuccessChanceModifier.Calculate(base.Context), 0, 100), 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}
}
