using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Add weapons for show")]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("4cc38c8a3e3ea5d44afe498c6fa228c3")]
public class AddCalculatedWeapon : UnitFactComponentDelegate
{
	public CalculatedWeapon Weapon;

	public bool ScaleDamageByRank;
}
