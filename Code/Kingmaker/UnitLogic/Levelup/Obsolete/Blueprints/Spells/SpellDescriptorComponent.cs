using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Levelup.Obsolete.Blueprints.Spells;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintBuff))]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintAreaEffect))]
[TypeId("91216784d99a12e428bf782c8a8c7f5c")]
public class SpellDescriptorComponent : BlueprintComponent
{
	public SpellDescriptorWrapper Descriptor;
}
