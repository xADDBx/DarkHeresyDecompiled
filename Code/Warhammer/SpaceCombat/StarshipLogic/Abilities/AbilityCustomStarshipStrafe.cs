using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[Obsolete]
[TypeId("c2bd270589e578143b8c700d785013eb")]
public class AbilityCustomStarshipStrafe : BlueprintComponent
{
	[SerializeField]
	private GameObject NodeMarker;

	[SerializeField]
	private float strafeTime = 0.25f;

	[SerializeField]
	private bool AllowBulking;

	[SerializeField]
	[Range(0f, 1f)]
	[ShowIf("AllowBulking")]
	private float visualCellPenetration;

	[SerializeField]
	[ShowIf("AllowBulking")]
	private float fallBackTime;

	[SerializeField]
	[ShowIf("AllowBulking")]
	private float RamDamageMod;

	[SerializeField]
	private ActionList StarshipActionsOnFinish;
}
