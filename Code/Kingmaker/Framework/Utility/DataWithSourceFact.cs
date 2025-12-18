using Kingmaker.Blueprints;
using Kingmaker.EntitySystem;

namespace Kingmaker.Framework.Utility;

public readonly struct DataWithSourceFact<T>
{
	public readonly T Data;

	public readonly EntityFactRef Fact;

	public readonly BlueprintComponentReference Component;

	public DataWithSourceFact(T data, EntityFact fact, BlueprintComponent component)
	{
		Data = data;
		Fact = fact;
		Component = component;
	}

	public DataWithSourceFact(T data, EntityFactComponent component)
		: this(data, component.Fact, component.SourceBlueprintComponent)
	{
	}

	public bool IsFrom(EntityFactComponent component)
	{
		if (Fact == component.Fact)
		{
			return Component == component.SourceBlueprintComponent;
		}
		return false;
	}

	public bool IsFrom(EntityFact fact, BlueprintComponent component)
	{
		if (Fact == fact)
		{
			return Component == component;
		}
		return false;
	}
}
