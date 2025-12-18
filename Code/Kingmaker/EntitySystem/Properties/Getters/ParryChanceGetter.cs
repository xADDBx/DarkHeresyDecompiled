using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("5fedba6883837ed43ab25613b07a422b")]
public class ParryChanceGetter : IntPropertyGetter
{
	public PropertyTargetType Attacker;

	public bool NoTarget;

	public bool OnlyNegativeModifiers;

	public bool DoNotCountWeaponSkillAndAgility;

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (NoTarget)
		{
			return "Parry of " + FormulaTargetScope.Current + " against abstract attack";
		}
		return "Parry of " + FormulaTargetScope.Current + " against " + Attacker.Colorized();
	}
}
