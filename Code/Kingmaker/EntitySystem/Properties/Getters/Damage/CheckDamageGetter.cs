using System;
using JetBrains.Annotations;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Kingmaker.Mechanics.Damage;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters.Damage;

[Serializable]
[TypeId("e4eb8339ae4b4ac1be10c0efb45500eb")]
public abstract class CheckDamageGetter : BoolPropertyGetter
{
	[EnumFlagsAsButtons(ColumnCount = 4)]
	public DamageTypeMask Types;

	[EnumFlagsAsButtons(ColumnCount = 4)]
	public DamageCategoryMask Categories;

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Check Damage Properties";
	}

	protected sealed override bool GetBaseValue()
	{
		if (!Check(out var type, out var _, out var _))
		{
			return false;
		}
		if (Types != 0 && (Types & type.GetInfo().Mask) == 0)
		{
			return false;
		}
		if (Categories != 0 && (Categories & type.GetInfo().CategoryMask) == 0)
		{
			return false;
		}
		return true;
	}

	protected abstract bool Check(out DamageType type, [CanBeNull] out IntermediateDamage data, [CanBeNull] out RulebookEvent rule);
}
