using System;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("e23ef463879e4908b8b6e43ff12f3379")]
public class AnomalyShipRun : GameAction
{
	public bool DisappearAfterRun;

	public override string GetCaption()
	{
		return "Interacting anomaly run to the edge of system";
	}

	protected override void RunAction()
	{
	}
}
