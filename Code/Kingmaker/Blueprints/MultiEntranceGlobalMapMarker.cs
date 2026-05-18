using Kingmaker.Blueprints.Attributes;
using Kingmaker.Globalmap.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintMultiEntranceEntry))]
[TypeId("a76f88e328fa9324e9680b10ae434d80")]
public class MultiEntranceGlobalMapMarker : BlueprintComponent
{
	public BlueprintMultiEntranceMap Zone;
}
