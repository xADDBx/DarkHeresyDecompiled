using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("07b44e4b8c5e4725ad007726c85820cb")]
public class NavigatorResourceEnough : Condition
{
	public int Count;

	protected override string GetConditionCaption()
	{
		return $"Check if player have at least {Count} of navigator resource";
	}

	protected override bool CheckCondition()
	{
		return false;
	}
}
