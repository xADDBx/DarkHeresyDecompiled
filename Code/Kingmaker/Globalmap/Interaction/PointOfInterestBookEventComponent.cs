using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints.Exploration;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Globalmap.Interaction;

[Obsolete]
[AllowMultipleComponents]
[TypeId("a43ec46b34714baeadccaffda2641bb8")]
public class PointOfInterestBookEventComponent : BasePointOfInterestComponent
{
	public new BlueprintPointOfInterestBookEvent PointBlueprint => (BlueprintPointOfInterestBookEvent)base.PointBlueprint;
}
