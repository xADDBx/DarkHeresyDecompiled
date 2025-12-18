using Kingmaker.Blueprints.Attributes;
using Kingmaker.Localization;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Blueprints;

[AllowedOn(typeof(BlueprintFeature))]
[TypeId("22763793998e4e87a8051557fb3b0040")]
public class AddForceSetName : BlueprintComponent
{
	public LocalizedString ForceName;
}
