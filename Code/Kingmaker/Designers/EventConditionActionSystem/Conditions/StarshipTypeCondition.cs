using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;
using Warhammer.SpaceCombat.Blueprints;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("5746726ed56046efb018aaceb7c7bcb1")]
public class StarshipTypeCondition : Condition
{
	public PlayerShipType StarshipType;

	protected override string GetConditionCaption()
	{
		return "Players starship is " + StarshipType.ToString() + " type";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
