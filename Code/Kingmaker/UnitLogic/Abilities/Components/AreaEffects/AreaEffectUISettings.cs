using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.AreaEffects;

[AllowedOn(typeof(BlueprintAreaEffect))]
[ComponentName("UI/AreaEffectUISettings")]
[TypeId("95ede9055d7440bf88cb0649c3e036f1")]
public class AreaEffectUISettings : BlueprintComponent
{
	public QuickInspectUISettings QuickInspectSettings;
}
