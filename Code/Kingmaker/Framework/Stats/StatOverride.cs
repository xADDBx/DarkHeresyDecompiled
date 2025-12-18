using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Stats.Base;

namespace Kingmaker.Framework.Stats;

public readonly struct StatOverride
{
	public readonly StatType Type;

	public readonly EntityFactRef Fact;

	public readonly BlueprintComponent? Component;

	public readonly EntityPart? Part;

	public readonly bool OnlyIfHigher;

	public StatOverride(StatType type, EntityFactComponent source, bool onlyIfHigher)
	{
		Type = type;
		Fact = source.Fact;
		Component = source.SourceBlueprintComponent;
		Part = null;
		OnlyIfHigher = onlyIfHigher;
	}

	public StatOverride(StatType type, EntityPart source, bool onlyIfHigher)
	{
		Type = type;
		Fact = null;
		Component = null;
		Part = source;
		OnlyIfHigher = onlyIfHigher;
	}
}
