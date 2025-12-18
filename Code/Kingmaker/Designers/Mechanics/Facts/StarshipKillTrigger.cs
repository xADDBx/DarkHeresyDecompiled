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
[TypeId("e504ccf75a2139b45a2fb1d50d2dabf0")]
public class StarshipKillTrigger : BlueprintComponent
{
	[SerializeField]
	private bool IgnoreSoftUnits;

	[SerializeField]
	private bool EnemyOnly;

	[SerializeField]
	private ActionList ActionsOnSelf;

	[SerializeField]
	private ActionList ActionsOnTarget;
}
