using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Enums;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c2eca07b264e7af46962536631163408")]
public class WarhammerOutgoingDodgeChancesBonus : UnitFactComponentDelegate
{
	public ContextValue Value;

	public bool OnlyAgainstCaster;

	public bool OnlyForAllies;

	public bool SpecificWeaponFamily;

	[ShowIf("SpecificWeaponFamily")]
	public WeaponFamily WeaponFamily = WeaponFamily.Bolt;
}
