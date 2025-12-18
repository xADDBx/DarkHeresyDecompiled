using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("6691fff49dc96ab479e16f105a740b76")]
public class StarshipAIBreakPlan : BlueprintComponent
{
}
