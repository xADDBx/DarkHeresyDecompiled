using System.Collections.Generic;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Blueprints.Facts;
using Kingmaker.Code.UnitLogic.Parts;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Enums;
using Kingmaker.Framework.Mechanics.Actor;
using Kingmaker.RuleSystem.Rules.Modifiers;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Mechanics;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.Designers.Mechanics.Facts;

[AllowedOn(typeof(BlueprintUnitFact))]
[TypeId("61431ca715b94fe1adeec85a9330da29")]
public class AddMachineTrait : UnitFactComponentDelegate, IStatModifier
{
	[SerializeField]
	private ContextValue m_Value = 0;

	protected override void OnActivateOrPostLoad()
	{
		base.Owner.Parts.GetOrCreate<PartMachineTrait>().Retain();
	}

	protected override void OnDeactivate()
	{
		base.Owner.Parts.GetOrCreate<PartMachineTrait>().Release();
	}

	void IStatModifier.TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context)
	{
		if (stat == StatType.MachineTrait)
		{
			collector.Modifiers.Add(ModifierType.ValAdd, m_Value.Calculate(base.Context), base.Runtime, ModifierDescriptor.UntypedUnstackable);
		}
	}

	void IStatModifier.CollectAffectedStats(ICollection<AffectedStatEntry> entries)
	{
		entries.Add(new AffectedStatEntry(StatType.MachineTrait));
	}
}
