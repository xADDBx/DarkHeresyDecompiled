using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[Obsolete]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("6132a0a1d20184f408ddae21ae475cee")]
public class RestrictedFiringAreaComponent : BlueprintComponent
{
	[SerializeField]
	private AoEPattern m_Pattern;

	public int RestrictedAngleDegrees => m_Pattern.Angle;
}
