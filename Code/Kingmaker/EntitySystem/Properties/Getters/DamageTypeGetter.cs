using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Framework;
using Kingmaker.RuleSystem.Rules.Damage;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("5acb988246cb416eaf70c152840975b3")]
public class DamageTypeGetter : BoolPropertyGetter, PropertyContextAccessor.IRule, PropertyContextAccessor.IRequired, PropertyContextAccessor.IBase
{
	public DamageType DamageType;

	protected override bool GetBaseValue()
	{
		return (EvalContext.Current.Rule as IDamageHolderRule)?.DamageType == DamageType;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return $"Check Damage type is {DamageType}";
	}
}
