using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintFeature))]
[AllowedOn(typeof(BlueprintFact))]
[AllowMultipleComponents]
[TypeId("a4cc53e785f45474f97119cab2150d54")]
public class StarshipRamModifiers : BlueprintComponent
{
	[SerializeField]
	public float DamageReturningMod;

	[SerializeField]
	public int RamDistanceBonus;
}
