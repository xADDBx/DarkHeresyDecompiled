using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.RuleSystem;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.UnitLogic.Buffs.Components;

[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[ComponentName("Stats/StatReductionBuff")]
[TypeId("a64c5f5bc18aa7e439187c400cbe5a38")]
public class StatReductionBuff : UnitBuffComponentDelegate
{
	public ModifierDescriptor Descriptor;

	public StatType Stat;

	public bool IsPercentReduction;

	[HideIf("IsPercentReduction")]
	public ContextValue ReductionValue;

	[HideInInspector]
	public DiceFormula Value;

	[HideInInspector]
	public int Bonus;

	[ShowIf("IsPercentReduction")]
	[Tooltip("The percentage by which the stat will be reduced")]
	[Range(0f, 100f)]
	public int ReductionPercent;

	protected override void OnActivateOrPostLoad()
	{
		ModifiableValue stat = base.Owner.Stats.GetStat(Stat);
		int num;
		if (IsPercentReduction)
		{
			num = stat.BaseValue * ReductionPercent / 100;
		}
		else
		{
			ContextValue reductionValue = ReductionValue;
			num = ((reductionValue == null || reductionValue.IsZero) ? ((Value.MinValue(Bonus) + Value.MaxValue(Bonus)) / 2) : ReductionValue.Calculate(base.Context));
		}
		stat.AddModifier(-num, base.Runtime, Descriptor);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Stat).RemoveModifiersFrom(base.Runtime);
	}
}
