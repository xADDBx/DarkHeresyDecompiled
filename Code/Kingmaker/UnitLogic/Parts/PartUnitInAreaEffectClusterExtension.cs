using JetBrains.Annotations;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.UnitLogic.Parts;

public static class PartUnitInAreaEffectClusterExtension
{
	[CanBeNull]
	public static PartUnitInAreaEffectCluster GetPartUnitInAreaEffectClusterOptional(this MechanicEntity entity)
	{
		return entity.GetOptional<PartUnitInAreaEffectCluster>();
	}

	public static bool HasCurrentClusterKey(this MechanicEntity entity, BlueprintAreaEffectClusterLogic blueprint)
	{
		return entity.GetPartUnitInAreaEffectClusterOptional()?.ClusterKeys.Contains(blueprint) ?? false;
	}

	public static bool IsCurrentlyInAnotherClusterArea(this MechanicEntity entity, BlueprintAreaEffectClusterLogic blueprint, AreaEffectEntity checkingAreaEffect)
	{
		PartUnitInAreaEffectCluster partUnitInAreaEffectClusterOptional = entity.GetPartUnitInAreaEffectClusterOptional();
		if (partUnitInAreaEffectClusterOptional == null)
		{
			return false;
		}
		partUnitInAreaEffectClusterOptional.AreaEffectEntitiesInVisit.TryGetValue(blueprint, out var value);
		if (value == null || value.Empty())
		{
			return false;
		}
		bool result = false;
		foreach (AreaEffectEntity item in value)
		{
			if (item != checkingAreaEffect && !item.IsEntityOutside(entity))
			{
				result = true;
			}
		}
		return result;
	}
}
