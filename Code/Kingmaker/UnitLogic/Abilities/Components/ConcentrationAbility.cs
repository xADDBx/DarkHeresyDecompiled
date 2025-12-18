using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("26455fc841ec4aee9d6a1f68bad45cfc")]
public class ConcentrationAbility : BlueprintComponent
{
	public int ActionPointCost;

	public int MovementPointCost;
}
