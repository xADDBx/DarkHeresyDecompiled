using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Assets.Code.Designers.Mechanics.Facts;

[Obsolete]
[ComponentName("Modifes number of starship weapon maximum charges")]
[AllowedOn(typeof(BlueprintFact))]
[AllowMultipleComponents]
[TypeId("47c0913bfcaa42a4da671693b84ef65b")]
public class StarshipModifyMaxCharges : BlueprintComponent
{
	[SerializeField]
	public StarshipWeaponType WeaponType;

	[SerializeField]
	public int Value;
}
