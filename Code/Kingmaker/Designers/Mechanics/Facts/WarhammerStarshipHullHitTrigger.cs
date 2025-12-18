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
[TypeId("c602a2cdfd853564cbc87ceef1e1e221")]
public class WarhammerStarshipHullHitTrigger : BlueprintComponent
{
	[SerializeField]
	public ActionList ActionsOnHullHit;

	[SerializeField]
	public bool ActivateIfInitiator;

	[SerializeField]
	public bool ActivateIfTarget;
}
