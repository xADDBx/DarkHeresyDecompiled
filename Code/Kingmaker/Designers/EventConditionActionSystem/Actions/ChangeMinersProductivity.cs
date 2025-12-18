using System;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Persistence.Versioning;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Actions;

[Obsolete]
[TypeId("adc099b2458f409482d08821e5b33baf")]
[PlayerUpgraderAllowed(false)]
public class ChangeMinersProductivity : GameAction
{
	public int m_ProductivityPercents;

	public override string GetCaption()
	{
		return "Change miner productivity";
	}

	protected override void RunAction()
	{
	}
}
