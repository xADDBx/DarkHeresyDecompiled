using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("9adedba45dfe4964fae8cca22e179e20")]
public class StarshipAbilityRangeExtender : BlueprintComponent
{
	[SerializeField]
	private bool AE_Only;

	[SerializeField]
	private int extraRange;
}
