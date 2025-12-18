using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[PlayerUpgraderAllowed(false)]
[TypeId("67ff1fc51e824dbbb6a3bbf44463bb1c")]
public class RecalculateCargo : GameAction
{
	public override string GetCaption()
	{
		return "Recalculate cargo after blueprint changes";
	}

	protected override void RunAction()
	{
	}
}
