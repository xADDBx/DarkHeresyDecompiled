using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Designers.Mechanics.Facts.Restrictions;
using Kingmaker.ElementsSystem;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("bd637fd86978432bae77b7a9cd4bb245")]
public class ChangeRuleOnAttack : BlueprintComponent
{
	[SerializeField]
	protected RestrictionCalculator Restrictions = new RestrictionCalculator();

	public bool ChangeDamageValue;

	[ShowIf("ChangeDamageValue")]
	public ContextValue ContextAdditionDamageValue;

	public bool ChangeCritChance;

	[ShowIf("ChangeCritChance")]
	public int AdditionCritChance;

	public bool ChangeCritDamage;

	[ShowIf("ChangeCritDamage")]
	public int AdditionCritDamage;

	public ActionList Actions;
}
