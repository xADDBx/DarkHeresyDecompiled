using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("8b570f3f321a21e4ba220a9d20cb6190")]
public class PsyRatingGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "PR of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		return 0;
	}
}
