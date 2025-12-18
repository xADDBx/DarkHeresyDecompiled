using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("b9dd49398cf14b469d7fc78f01eb5e85")]
public class CoverStatusGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Cover type of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		return 0;
	}
}
