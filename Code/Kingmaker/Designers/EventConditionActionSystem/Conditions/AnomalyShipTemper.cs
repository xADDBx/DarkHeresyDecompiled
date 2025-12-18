using System;
using Kingmaker.ElementsSystem;
using Kingmaker.Globalmap.Exploration;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("b2052f518577440783677317663cac36")]
public class AnomalyShipTemper : Condition
{
	public AnomalyStarShip.ShipTemper Temper;

	protected override string GetConditionCaption()
	{
		return "Check temper of currently interacted anomaly";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
