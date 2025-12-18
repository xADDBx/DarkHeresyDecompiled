using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.Gameplay.Features.AreaEffects;

public static class AreaEffectPredictionHelper
{
	private static readonly Dictionary<BlueprintAreaEffect, AreaEffectDescription> _cache = new Dictionary<BlueprintAreaEffect, AreaEffectDescription>();

	public static AreaEffectDescription GetEffectDescription(this BlueprintAreaEffect areaEffect)
	{
		if (!_cache.TryGetValue(areaEffect, out AreaEffectDescription value))
		{
			value = (_cache[areaEffect] = new AreaEffectDescription(areaEffect));
		}
		return value;
	}

	public static AreaEffectDescription GetEffectDescription(this AreaEffectEntity areaEffect)
	{
		return areaEffect.Blueprint.GetEffectDescription();
	}

	public static bool IsHarmful(this BlueprintAreaEffect areaEffect)
	{
		return areaEffect.GetEffectDescription().IsHarmful();
	}

	public static bool IsHarmful(this AreaEffectEntity areaEffect)
	{
		return areaEffect.GetEffectDescription().IsHarmful();
	}
}
