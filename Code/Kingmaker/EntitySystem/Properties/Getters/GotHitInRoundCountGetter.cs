using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[Obsolete]
[TypeId("36c5fd7a9911a3d4cb7d3f893323b206")]
public class GotHitInRoundCountGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetCombatStateOptional()?.GotHitInRoundCount ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Count of hits on " + FormulaTargetScope.Current + " this round";
	}
}
