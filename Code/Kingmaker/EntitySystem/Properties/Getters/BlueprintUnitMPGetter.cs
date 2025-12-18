using System;
using Kingmaker.Controllers.Combat;
using Kingmaker.EntitySystem.Properties.BaseGetter;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.EntitySystem.Properties.Getters;

[Obsolete]
[TypeId("0d38a4a1c629a0c4ab887cc077b5e95f")]
public class BlueprintUnitMPGetter : IntPropertyGetter
{
	protected override int GetBaseValue()
	{
		return base.CurrentEntity.GetCombatStateOptional()?.MovementPointsMax ?? 0;
	}

	protected override string GetInnerCaption(bool useLineBreaks)
	{
		return "WarhammerInitialAPBlue from " + FormulaTargetScope.Current + " blueprint";
	}
}
