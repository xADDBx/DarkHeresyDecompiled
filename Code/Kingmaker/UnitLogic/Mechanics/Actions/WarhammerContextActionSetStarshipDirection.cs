using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Properties;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("7b639717dfd57bb469d3b87e09d5f61e")]
public class WarhammerContextActionSetStarshipDirection : ContextAction
{
	private enum RotationType
	{
		FixedAngle,
		RandomAngle
	}

	[SerializeField]
	private RotationType Rotation;

	[SerializeField]
	private PropertyCalculator Angle;

	[SerializeField]
	private int maximalTargetInertiaToApplyLowInertiaAngle = -1;

	[SerializeField]
	private PropertyCalculator LowInertiaAngle;

	[SerializeField]
	private ActionList ActionsOnClockwiseTurn;

	[SerializeField]
	private ActionList ActionsOnCounterTurn;

	[SerializeField]
	private ActionList ActionsOnNoTurn;

	[SerializeField]
	[Tooltip("Damage is done as percent of max HP, modified with already taken damage, one instance for each 45 turn, cumulative")]
	private int damageBaseMin;

	[SerializeField]
	private int damageBaseMax;

	public override string GetCaption()
	{
		return Rotation switch
		{
			RotationType.FixedAngle => $"Turn by an angle of {Angle}", 
			RotationType.RandomAngle => $"Turn by a random angle from 0 to {Angle}", 
			_ => "<unknown rotation type>", 
		};
	}

	protected override void RunAction()
	{
	}
}
