using System;
using Kingmaker.Controllers.TurnBased;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Buffs;

[Obsolete]
[TypeId("05c28d3204531454b879d7acf60631ae")]
public class BuffSaveEachRound : UnitBuffComponentDelegate, ITickEachRound
{
	public SavingThrowType SaveType;

	public void OnNewRound()
	{
		RulePerformSavingThrow obj = new RulePerformSavingThrow(base.Owner, SaveType, 0)
		{
			Reason = base.Fact
		};
		Rulebook.Trigger(obj);
		if (obj.IsPassed)
		{
			base.Buff.Remove();
		}
	}
}
