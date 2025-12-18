using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("8d6d441ffcdf3e24a88daf5a16f31586")]
public class StarShipRapidReloadController : BlueprintComponent
{
	[SerializeField]
	private StarshipWeaponType m_weaponType;

	[SerializeField]
	private StarshipWeaponType m_weaponPenaltedType;
}
