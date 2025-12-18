using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.UnitLogic.Buffs.Blueprints;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.Mechanics.Buffs;

[Obsolete]
[ComponentName("Add stat bonus from ability value")]
[AllowedOn(typeof(BlueprintBuff))]
[AllowMultipleComponents]
[TypeId("c6f48aa9766288d408eeac7a9f767c74")]
public class AddStatBonusAbilityValue : UnitBuffComponentDelegate
{
	public ModifierDescriptor Descriptor;

	public StatType Stat;

	public ContextValue Value;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Stats.GetStat(Stat)?.AddModifier(Value.Calculate(base.Context), base.Runtime, Descriptor);
	}

	protected override void OnDeactivate()
	{
		base.Owner.Stats.GetStat(Stat)?.RemoveModifiersFrom(base.Runtime);
	}
}
