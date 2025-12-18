using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.FactLogic;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.Buffs.Components;

[Obsolete]
[ComponentName("Buffs/Generic stat bonus")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("a0c7e67ead2fb1b469484150184b3d4a")]
public class AddGenericStatBonus : UnitBuffComponentDelegate
{
	public ModifierDescriptor Descriptor;

	public StatType Stat;

	public int Value = 1;

	protected override void OnActivateOrPostLoad()
	{
		int value = AddStatBonus.TryApplyArcanistPowerfulChange(base.Context, Stat, Value);
		base.Owner.Stats.GetStat(Stat)?.AddModifier(value, base.Runtime, Descriptor);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Stat)?.RemoveModifiersFrom(base.Runtime);
	}
}
