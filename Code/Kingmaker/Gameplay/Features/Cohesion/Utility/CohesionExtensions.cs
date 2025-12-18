using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;

namespace Kingmaker.Gameplay.Features.Cohesion.Utility;

public static class CohesionExtensions
{
	public static bool IsCohesionRange(this BlueprintAreaEffect areaEffect)
	{
		return ConfigRoot.Instance.CombatRoot.CohesionAreaEffect == areaEffect;
	}

	public static bool IsCohesionRange(this AreaEffectEntity areaEffect)
	{
		return areaEffect.Blueprint.IsCohesionRange();
	}
}
