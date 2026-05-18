using System.Collections.Generic;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Mechanics.Actor;

public interface IStatModifier
{
	StatModifierScope Scope => StatModifierScope.Owner;

	void TryApplyStatModifier(StatModifierCollector collector, StatType stat, StatContext context);

	void CollectAffectedStats(ICollection<AffectedStatEntry> entries);
}
