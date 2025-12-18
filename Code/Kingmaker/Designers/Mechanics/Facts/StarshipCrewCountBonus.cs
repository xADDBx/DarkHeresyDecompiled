using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[TypeId("aab1d9de84cd4cb5a934ac4d7ab7f1bd")]
public class StarshipCrewCountBonus : BlueprintComponent
{
	[SerializeField]
	private ShipModuleType m_ModuleType;

	[SerializeField]
	private int m_Bonus;
}
