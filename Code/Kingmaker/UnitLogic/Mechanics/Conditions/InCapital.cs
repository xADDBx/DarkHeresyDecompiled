using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Conditions;

[Obsolete]
[TypeId("fb70cfcd42c67f248814695496dcd91b")]
public class InCapital : Condition
{
	protected override string GetConditionCaption()
	{
		return "In Capital";
	}

	protected override bool CheckCondition()
	{
		return Game.Instance.LoadedAreaState.Settings.CapitalPartyMode;
	}
}
