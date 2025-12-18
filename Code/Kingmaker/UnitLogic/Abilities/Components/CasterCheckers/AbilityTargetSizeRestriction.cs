using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[AllowMultipleComponents]
[TypeId("7a7b0d6d0475eec458b26ba01752c36c")]
public class AbilityTargetSizeRestriction : BlueprintComponent
{
	public Size[] AllowedSizes;

	public Size[] ForbiddenSizes;
}
