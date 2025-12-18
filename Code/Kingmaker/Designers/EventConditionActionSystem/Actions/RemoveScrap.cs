using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("fa408806425b4c56a0bc5c7770a2ff60")]
[PlayerUpgraderAllowed(false)]
public class RemoveScrap : GameAction
{
	public int Scrap;

	public override string GetCaption()
	{
		return "Take scrap from player";
	}

	protected override void RunAction()
	{
	}
}
