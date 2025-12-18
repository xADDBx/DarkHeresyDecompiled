using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Increase by 1 attacks count in burst for choosen weapon group")]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintFact))]
[AllowMultipleComponents]
[TypeId("7cf44657d29b8e548b28bfc8f2f26fdc")]
public class StarshipBurstAttackCountIncrease : BlueprintComponent
{
	[SerializeField]
	public StarshipWeaponType weaponType;

	[SerializeField]
	public int Chances;
}
