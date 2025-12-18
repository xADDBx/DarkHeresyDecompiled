using System;
using Kingmaker.SpaceCombat.StarshipLogic.Parts;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("5a62434637b179e448bf2f822d6eb945")]
public class WarhammerContextActionSetStarshipSpeedMode : ContextAction
{
	[SerializeField]
	private PartStarshipNavigation.SpeedModeType SpeedMode;

	public override string GetCaption()
	{
		return $"Set speed mode to {SpeedMode}";
	}

	protected override void RunAction()
	{
	}
}
