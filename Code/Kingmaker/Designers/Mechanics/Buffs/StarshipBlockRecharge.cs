using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Warhammer.SpaceCombat.Blueprints;
using Warhammer.SpaceCombat.Blueprints.Slots;

namespace Kingmaker.Designers.Mechanics.Buffs;

[Obsolete]
[ComponentName("Blocks recharging of selected starship weapon type")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("92510e08f7ed17544a7bc96d6941fb19")]
public class StarshipBlockRecharge : BlueprintComponent
{
	public StarshipWeaponType WeaponType;

	public bool CheckSlot;

	[ShowIf("CheckSlot")]
	public WeaponSlotType slot;
}
