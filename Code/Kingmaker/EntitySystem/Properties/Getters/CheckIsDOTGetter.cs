using System;
using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine.Serialization;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5fb4a7ae99fd47f99ef012e9f4700ba0")]
public sealed class CheckIsDOTGetter : BoolPropertyGetter, PropertyContextAccessor.IOptionalRule, PropertyContextAccessor.IOptional, PropertyContextAccessor.IBase
{
	[FormerlySerializedAs("ByType")]
	public bool CheckType;

	[ShowIf("CheckType")]
	public DOT Type;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (!CheckType)
		{
			return "Damage is DOT";
		}
		return $"Damage is {Type} DOT";
	}

	protected override bool GetBaseValue()
	{
		DOTLogic dOTLogic = GetDOTLogic(EvalContext.Current.Rule);
		if (dOTLogic == null)
		{
			return false;
		}
		if (CheckType)
		{
			return dOTLogic.Type == Type;
		}
		return true;
	}

	private static DOTLogic? GetDOTLogic(RulebookEvent? rule)
	{
		if (!(rule is RuleCalculateDamage) && !(rule is RuleRollDamage) && !(rule is RuleDealDamage))
		{
			return null;
		}
		return rule.Reason.Fact?.Blueprint.GetComponent<DOTLogic>();
	}
}
