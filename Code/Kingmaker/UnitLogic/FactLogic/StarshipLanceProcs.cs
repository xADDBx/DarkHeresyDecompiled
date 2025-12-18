using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.FactLogic;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("7c2b3991e33fb6640b4f4680441de4be")]
public class StarshipLanceProcs : BlueprintComponent
{
	public int onEnemyProcChances;

	public int onSelfProcChances;

	[SerializeField]
	private ActionList ActionsOnProc;
}
