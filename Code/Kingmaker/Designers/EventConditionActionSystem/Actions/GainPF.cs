using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("f989ed3c11784a4292ab1934fab712f7")]
[PlayerUpgraderAllowed(false)]
public class GainPF : GameAction
{
	public float Value;

	public override string GetCaption()
	{
		return $"Change total PF on {Value}";
	}

	protected override void RunAction()
	{
	}
}
