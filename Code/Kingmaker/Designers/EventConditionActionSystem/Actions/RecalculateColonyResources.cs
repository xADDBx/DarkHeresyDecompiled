using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("85942dabc27a46958eda38ecb735761d")]
public class RecalculateColonyResources : GameAction
{
	public override string GetCaption()
	{
		return "For player upgrader only!";
	}

	protected override void RunAction()
	{
	}
}
