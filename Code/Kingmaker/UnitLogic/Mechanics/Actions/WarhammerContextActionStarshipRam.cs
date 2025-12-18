using System;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Mechanics.Actions;

[Obsolete]
[TypeId("1dff6f53c4285d14eaf190d693b96a5a")]
public class WarhammerContextActionStarshipRam : ContextAction
{
	public int Damage;

	public float BonusDamagePerCoveredCell = 1f;

	public override string GetCaption()
	{
		return "Starship Ram";
	}

	protected override void RunAction()
	{
	}
}
