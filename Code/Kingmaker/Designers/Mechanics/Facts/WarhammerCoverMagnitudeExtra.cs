using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("a890259ae728aa2478c1c8b387b7a1ae")]
public class WarhammerCoverMagnitudeExtra : UnitFactComponentDelegate
{
	public float CoverMagnitudeExtra;
}
