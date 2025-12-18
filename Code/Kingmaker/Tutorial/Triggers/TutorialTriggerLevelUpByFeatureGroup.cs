using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Tutorial.Triggers;

[Obsolete]
[TypeId("1de688dd70524edc9f5a9e250a2b6a9a")]
public class TutorialTriggerLevelUpByFeatureGroup : BlueprintComponent
{
	[Flags]
	public enum StarshipFeatureGroup
	{
		UltimateAbility = 2,
		ActiveAbility = 4,
		AdvancedAbility = 8,
		ShipUpgrade = 0x10
	}

	[SerializeField]
	public StarshipFeatureGroup StarshipFeatureGroups;
}
