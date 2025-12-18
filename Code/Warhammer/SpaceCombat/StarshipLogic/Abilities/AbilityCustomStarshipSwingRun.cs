using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[Obsolete]
[TypeId("f0d435adc40f01e408e6f4292f63b498")]
public class AbilityCustomStarshipSwingRun : BlueprintComponent
{
	[SerializeField]
	private ActionList StarshipActionsOnFinish;

	[SerializeField]
	private GameObject PassThroughMarker;

	[SerializeField]
	private GameObject FinalNodeMarker;

	[SerializeField]
	private bool IsUpgraded;
}
