using System;
using Kingmaker.Blueprints;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[Obsolete]
[TypeId("548ecad36993cc4499c5d2534ec91c13")]
public class AbilityCustomStarshipUnguidedTorpedoSetCourse : BlueprintComponent
{
	[SerializeField]
	private AoEPattern m_Pattern;

	[SerializeField]
	private GameObject PassThroughMarker;
}
