using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.RuleSystem;
using Kingmaker.RuleSystem.Rules;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Facts;

[Obsolete]
[AllowMultipleComponents]
[AllowedOn(typeof(BlueprintBuff))]
[TypeId("0b95b4897594493996d8d38142453fd0")]
public class BuffSavingThrowOnApplication : UnitBuffComponentDelegate
{
	public SavingThrowType SavingThrowType;

	public int DifficultyModifier;

	protected override void OnActivate()
	{
		RulePerformSavingThrow rulePerformSavingThrow = new RulePerformSavingThrow(base.Owner, SavingThrowType, DifficultyModifier);
		Rulebook.Trigger(rulePerformSavingThrow);
		if (rulePerformSavingThrow.IsPassed)
		{
			base.Buff.Remove();
		}
	}
}
