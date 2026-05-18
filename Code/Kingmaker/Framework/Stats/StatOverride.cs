using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Stats;

public readonly struct StatOverride
{
	public readonly StatType Type;

	public readonly EntityFactRef SourceFact;

	public readonly BlueprintComponent? SourceComponent;

	public readonly EntityPart? SourcePart;

	public readonly bool OnlyIfHigher;

	public StatOverride(StatType type, EntityFactComponent source, bool onlyIfHigher)
	{
		Type = type;
		SourceFact = source.Fact;
		SourceComponent = source.SourceBlueprintComponent;
		SourcePart = null;
		OnlyIfHigher = onlyIfHigher;
	}

	public StatOverride(StatType type, EntityPart source, bool onlyIfHigher)
	{
		Type = type;
		SourceFact = null;
		SourceComponent = null;
		SourcePart = source;
		OnlyIfHigher = onlyIfHigher;
	}
}
