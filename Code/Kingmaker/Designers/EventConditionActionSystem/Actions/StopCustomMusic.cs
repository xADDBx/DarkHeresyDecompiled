using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("102f30d37c318c146a007165d4f34418")]
public class StopCustomMusic : GameAction
{
	protected override void RunAction()
	{
	}

	public override string GetCaption()
	{
		return "Stop custom music";
	}
}
