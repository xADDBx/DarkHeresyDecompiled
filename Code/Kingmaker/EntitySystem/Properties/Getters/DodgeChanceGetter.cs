using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("f36866d65515f6d47be143a981139a42")]
public class DodgeChanceGetter : IntPropertyGetter
{
	public PropertyTargetType Attacker;

	public bool NoTarget;

	public bool OnlyNegativeModifiers;

	public bool DoNotCountPerception;

	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		if (NoTarget)
		{
			return "Dodge of " + FormulaTargetScope.Current + " against abstract attack";
		}
		return "Dodge of " + FormulaTargetScope.Current + " against " + Attacker.Colorized();
	}
}
