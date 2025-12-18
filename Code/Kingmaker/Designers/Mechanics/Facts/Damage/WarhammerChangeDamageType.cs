using System;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.UnitLogic.Mechanics.Damage;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts.Damage;

[Obsolete]
[TypeId("302fefe965a37924fb07780e26d425d7")]
public class WarhammerChangeDamageType : BlueprintComponent
{
	public RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool CheckDamageType;

	[SerializeField]
	[ShowIf("CheckDamageType")]
	private DamageType m_InitialDamageType;

	public DamageType OverrideDamageType;
}
