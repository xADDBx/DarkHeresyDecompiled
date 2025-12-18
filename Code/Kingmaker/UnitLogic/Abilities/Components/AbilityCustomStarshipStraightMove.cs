using System;
using Kingmaker.Blueprints;
using Kingmaker.EntitySystem.Properties;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Abilities.Components;

[Obsolete]
[TypeId("fcda3e2bc7ac9e44c90980b38725dffc")]
public class AbilityCustomStarshipStraightMove : BlueprintComponent
{
	[SerializeField]
	private PropertyCalculator straightMoveLength;

	[SerializeField]
	private PropertyCalculator movePointsToSpend;
}
