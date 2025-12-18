using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("a43b161334384249a5c76830c973d88c")]
public abstract class DescriptionModifier : BlueprintComponent
{
	public abstract string Modify(string originalString);
}
