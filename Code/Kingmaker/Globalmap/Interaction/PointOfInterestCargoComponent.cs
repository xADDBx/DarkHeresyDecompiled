using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Interaction;

[Obsolete]
[AllowMultipleComponents]
[TypeId("3ad85baec230fcf4a9b0f4bc9bd6aa80")]
public class PointOfInterestCargoComponent : BasePointOfInterestComponent
{
	public new BlueprintPointOfInterestCargo PointBlueprint => (BlueprintPointOfInterestCargo)base.PointBlueprint;
}
