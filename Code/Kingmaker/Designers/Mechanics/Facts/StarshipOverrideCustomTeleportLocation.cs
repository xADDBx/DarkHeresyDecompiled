using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("03dfa0208161dbc44a0603b03369ff4b")]
public class StarshipOverrideCustomTeleportLocation : BlueprintComponent
{
	private enum OverrideMode
	{
		OverideForRamPosition
	}

	[SerializeField]
	private OverrideMode m_OverrideMode;

	[SerializeField]
	private int distanceLimit = -1;
}
