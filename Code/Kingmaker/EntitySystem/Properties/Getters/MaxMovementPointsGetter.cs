using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("c8ec1a8f34944ea68f48a475ea7e5e60")]
public class MaxMovementPointsGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "Max MP of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetCombatStateOptional()?.MovementPointsMax ?? 0;
	}
}
