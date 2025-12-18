using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("55a9ea4dc50c2e14ab72fa4526e74a84")]
public class WarhammerSpaceCombatEndTrigger : BlueprintComponent
{
	[SerializeField]
	public ActionList ActionsOnEnd;
}
