using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[AllowedOn(typeof(BlueprintFeature))]
[AllowMultipleComponents]
[TypeId("058e37ed1490441995e349f52d8bb440")]
public abstract class NameModifier : BlueprintComponent
{
	public abstract string Modify(string originalString);
}
