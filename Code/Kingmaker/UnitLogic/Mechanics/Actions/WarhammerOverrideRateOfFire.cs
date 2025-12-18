using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[AllowedOn(typeof(BlueprintAbility))]
[TypeId("c0f9023b38c27ac45acdc2624b05543e")]
public class WarhammerOverrideRateOfFire : MechanicEntityFactComponentDelegate
{
	[SerializeField]
	private int m_RateOfFire;

	public int RateOfFire => Math.Max(1, m_RateOfFire);
}
