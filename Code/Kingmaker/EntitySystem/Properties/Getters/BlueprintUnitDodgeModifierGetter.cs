using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("f4f63f7f9e823c14ca9ce3c2684e51ee")]
public class BlueprintUnitDodgeModifierGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "DodgeModifier from " + FormulaTargetScope.Current + " blueprint";
	}
}
