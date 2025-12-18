using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Code.Gameplay.Components;

[AllowedOn(typeof(BlueprintUnit))]
[TypeId("3d5927eaa23e457a9afb2818235a55e9")]
public class UnitUISettings : BlueprintComponent
{
	public bool OverrideHideName;
}
