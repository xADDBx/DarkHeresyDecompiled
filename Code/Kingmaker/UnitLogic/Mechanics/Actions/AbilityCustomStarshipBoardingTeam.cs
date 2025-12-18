using System;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("1be3e39bdea40cc4a8a1322ad5166eba")]
public class AbilityCustomStarshipBoardingTeam : BlueprintComponent
{
	[SerializeField]
	private BlueprintProjectileReference m_ForwardProjectile;

	[SerializeField]
	private int ForwardCountMin;

	[SerializeField]
	private int ForwardCountMax;

	[SerializeField]
	private float ForwardIntervalMin;

	[SerializeField]
	private float ForwardIntervalMax;

	[SerializeField]
	private BlueprintProjectileReference m_ReturnProjectile;

	[SerializeField]
	private int ReturnCountMin;

	[SerializeField]
	private int ReturnCountMax;

	[SerializeField]
	private float ReturnIntervalMin;

	[SerializeField]
	private float ReturnIntervalMax;

	[SerializeField]
	private float delayBeforeReturning;

	[SerializeField]
	private float delayBeforeBoardingEnds;

	[SerializeField]
	private ActionList ActionsOnArrival;

	[SerializeField]
	private ActionList ActionsInTheMiddle;

	[SerializeField]
	private ActionList ActionsOnEnding;

	public BlueprintProjectile ForwardProjectile => m_ForwardProjectile?.Get();

	public BlueprintProjectile ReturnProjectile => m_ReturnProjectile?.Get();
}
