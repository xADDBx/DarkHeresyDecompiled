using System;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("76a29dc4ed46a474b8fceb18690879bd")]
public class CanActGetter : IntPropertyGetter
{
	public PropertyTargetType Target;

	protected override int GetBaseValue()
	{
		if (!base.CurrentEntity.CanAct)
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Can " + FormulaTargetScope.Current + " act";
	}
}
