using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("4ab8c580c9281bc47a38915cde4fe704")]
public class StarShipRapidReloadAction : ContextAction
{
	public bool AllowPenalted;

	[ShowIf("AllowPenalted")]
	public ActionList ActionsWhenReloadedPenalted;

	public bool AllowReloadAtAccelerationPhase;

	[ShowIf("AllowReloadAtAccelerationPhase")]
	public StarshipWeaponType ReloadWeapon;

	public override string GetCaption()
	{
		return "Spaceship RapidReload action";
	}

	protected override void RunAction()
	{
	}
}
