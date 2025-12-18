using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.UnitLogic.Abilities.Components;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("dee632de035bfdb48b823d7418a3ccd8")]
public class WarhammerModifyFiringArcRange : BlueprintComponent
{
	[SerializeField]
	private RestrictedFiringArc firingArc;

	[SerializeField]
	private int bonusRange;
}
