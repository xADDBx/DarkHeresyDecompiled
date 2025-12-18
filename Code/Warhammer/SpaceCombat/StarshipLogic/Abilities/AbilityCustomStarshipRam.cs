using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Warhammer.SpaceCombat.StarshipLogic.Abilities;

[Obsolete]
[TypeId("e8e5532868cba7f49a810fbd4d383a13")]
public class AbilityCustomStarshipRam : BlueprintComponent
{
	[SerializeField]
	private int minDistance;

	[SerializeField]
	private int bonusDistanceOnAttackAttempt;

	[SerializeField]
	[Range(0f, 1f)]
	private float visualCellPenetration;

	[SerializeField]
	[Range(0f, 1f)]
	private float onHitActionsCellPenetration = 1f;

	[SerializeField]
	private float fallBackTime;

	[SerializeField]
	private GameObject PassThroughMarker;

	[SerializeField]
	private GameObject FinalNodeMarker;

	[SerializeField]
	private BlueprintFeatureReference m_DamageBonusTalent;

	[SerializeField]
	private BlueprintFeatureReference m_FirestartingFeature;

	[SerializeField]
	[Tooltip("\ufffd\ufffd\ufffd \ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd \ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd \ufffd\ufffd\ufffd\ufffd, \ufffd\ufffd\ufffd\ufffd\ufffd\ufffd \ufffd\ufffd\ufffd\ufffd\ufffd, \ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd \ufffd \ufffd\ufffd \ufffd\ufffd\ufffd\ufffd\ufffd\ufffd\ufffd")]
	private bool AllowAutotarget;

	[SerializeField]
	private ActionList ActionsOnHitCaster;

	[SerializeField]
	private ActionList RepeatedBySizeActionsOnTarget;

	public int MinDistance => minDistance;
}
