using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("c9c11482ab60d5544b32f549619f6818")]
public class WarhammerDodgeModifier : UnitFactComponentDelegate
{
	public float dodgeModifer = 1f;

	[Range(0f, 1f)]
	public float dodgeMinimumEnsured;
}
