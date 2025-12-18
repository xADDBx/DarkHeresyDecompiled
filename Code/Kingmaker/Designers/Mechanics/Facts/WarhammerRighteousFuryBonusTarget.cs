using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("655d56d2a908e0846928313013cf0fc9")]
public class WarhammerRighteousFuryBonusTarget : UnitFactComponentDelegate
{
	public ContextValue Value;

	public ContextValue Multiplier;

	public bool SpecificRangeType;

	[ShowIf("SpecificRangeType")]
	public WeaponRangeType WeaponRangeType;

	public bool OnlyFromSpotWeaknessSide;

	[SerializeField]
	[ShowIf("OnlyFromSpotWeaknessSide")]
	private BlueprintBuffReference m_SpotWeaknessBuff;

	public BlueprintBuff SpotWeaknessBuff => m_SpotWeaknessBuff.Get();
}
