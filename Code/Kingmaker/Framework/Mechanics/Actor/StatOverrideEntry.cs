using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Mechanics.Actor;

public readonly struct StatOverrideEntry
{
	public readonly StatType Stat;

	public readonly bool OnlyIfHigher;

	public readonly EntityFact? Source;

	public StatOverrideEntry(StatType stat, bool onlyIfHigher, EntityFact? source)
	{
		Stat = stat;
		OnlyIfHigher = onlyIfHigher;
		Source = source;
	}
}
