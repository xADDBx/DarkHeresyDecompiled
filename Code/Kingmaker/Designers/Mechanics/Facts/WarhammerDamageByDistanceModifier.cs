using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("a04cead65e3d0da48b734c314e67f4e7")]
public class WarhammerDamageByDistanceModifier : BlueprintComponent
{
	public ContextValue DamageModifierPerCell = 0;

	public ContextValue PenetrationModifierPerCell = 0;
}
