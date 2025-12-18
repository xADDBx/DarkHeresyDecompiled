using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Serializable]
[TypeId("24ff12aa2865ed7449fa2a5b268a8e5b")]
public class CurrentMovementPointsGetter : IntPropertyGetter
{
	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "MP of " + FormulaTargetScope.Current;
	}

	protected override int GetBaseValue()
	{
		return (int)(base.CurrentEntity.GetCombatStateOptional()?.MovementPoints ?? 0f);
	}
}
