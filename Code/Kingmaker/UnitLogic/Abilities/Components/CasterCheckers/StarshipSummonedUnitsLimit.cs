using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Abilities.Components.CasterCheckers;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("859d4203501070e4b8671e7f49fb2da2")]
public class StarshipSummonedUnitsLimit : BlueprintComponent
{
	[SerializeField]
	[Tooltip("Reference to blueprint of summoned unit to count")]
	private BlueprintStarship.Reference unit;

	[SerializeField]
	private int limit;

	[SerializeField]
	private BlueprintFeatureReference m_ExpansionFeature;
}
