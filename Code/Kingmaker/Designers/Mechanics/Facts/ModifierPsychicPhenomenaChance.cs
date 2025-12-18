using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("1bd8f9b17dea44eebf0ed90c8dc7cdcc")]
public class ModifierPsychicPhenomenaChance : UnitFactComponentDelegate
{
	public ContextValue AdditionChanceOnPsychicPhenomena;

	public ContextValue AdditionChanceOnPerilsOfWarp;
}
