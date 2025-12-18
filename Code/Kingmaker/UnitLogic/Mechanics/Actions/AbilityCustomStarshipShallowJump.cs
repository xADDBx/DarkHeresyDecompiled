using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.ResourceLinks;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("90148a340eede5946ade283656c21fe1")]
public class AbilityCustomStarshipShallowJump : BlueprintComponent
{
	public int MinDistance;

	public int MaxDistance;

	public float ArrivalDelay;

	public PrefabLink ExitPortalPrefab;

	public int jumpedOverPctDamageMin;

	public int jumpedOverPctDamageMax;

	[SerializeField]
	private ActionList StarshipActionsOnFinish;
}
