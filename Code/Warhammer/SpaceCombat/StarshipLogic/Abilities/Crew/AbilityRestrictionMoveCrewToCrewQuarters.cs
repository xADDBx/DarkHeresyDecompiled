using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities.Crew;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("83e58ba87dbb49608c53011182f96c53")]
public class AbilityRestrictionMoveCrewToCrewQuarters : BlueprintComponent
{
	[SerializeField]
	private ShipModuleType m_ModuleType;
}
