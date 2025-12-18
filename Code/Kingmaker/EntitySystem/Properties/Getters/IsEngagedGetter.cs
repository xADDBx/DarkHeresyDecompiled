using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("c697e468a142eef4cb78e41e83a7a4a2")]
public class IsEngagedGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		if (!base.CurrentEntity.IsEngagedInMelee())
		{
			return 0;
		}
		return 1;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Is " + FormulaTargetScope.Current + " engaged in melee combat.";
	}
}
