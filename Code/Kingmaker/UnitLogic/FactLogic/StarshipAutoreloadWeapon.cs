using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("fab5c718491ba8343aec71c233f7c8bd")]
public class StarshipAutoreloadWeapon : BlueprintComponent
{
	public StarshipWeaponType WeaponType;

	public ActionList ActionsOnReload;
}
