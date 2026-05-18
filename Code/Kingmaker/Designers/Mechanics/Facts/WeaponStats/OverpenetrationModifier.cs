using System;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.Enums;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.WeaponStats;

[Serializable]
[TypeId("f8c6ba4c7f6d4d25ac8c56aac2fecbec")]
public abstract class OverpenetrationModifier : MechanicEntityFactComponentDelegate
{
	[SerializeField]
	private RestrictionCalculator m_Restrictions = new RestrictionCalculator();

	[SerializeField]
	private ModifierDescriptor m_Descriptor;

	[SerializeField]
	private ContextValueModifierWithType m_OverpenetrationFactor;

	public void Apply(RuleCalculateStatsWeapon rule)
	{
	}
}
