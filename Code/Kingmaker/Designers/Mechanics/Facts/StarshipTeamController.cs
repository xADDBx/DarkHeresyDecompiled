using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintStarship))]
[TypeId("c679873c7dfdc494bbf909ab871de9cb")]
public class StarshipTeamController : BlueprintComponent
{
	[SerializeField]
	private int HpPerUnit = 1;

	[SerializeField]
	private bool CheckLanding;

	[SerializeField]
	[ShowIf("CheckLanding")]
	private int LandingDistance;

	[SerializeField]
	[ShowIf("CheckLanding")]
	private ActionList LandingActions;
}
