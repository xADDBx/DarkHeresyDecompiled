using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("723d037abc23ced4fb02c9cb299d1659")]
public class CheckIsSoftStarshipGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return FormulaTargetScope.Current + " is SOFT starship";
	}
}
