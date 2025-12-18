using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("433d4bafc1ae7c140bfc3dedc8c578c3")]
public class StarshipStrikecraftLogic : BlueprintComponent
{
	[SerializeField]
	private int m_ReturningBrainIndex;

	[SerializeField]
	private bool m_ExpendAllFuelOnAttack;

	[SerializeField]
	private BlueprintBuffReference m_FuelBuff;

	[SerializeField]
	private BlueprintBuffReference m_LandingBuff;

	public ActionList ExpirationActions;

	public BlueprintBuff FuelBuff => m_FuelBuff?.Get();

	public BlueprintBuff LandingBuff => m_LandingBuff?.Get();
}
