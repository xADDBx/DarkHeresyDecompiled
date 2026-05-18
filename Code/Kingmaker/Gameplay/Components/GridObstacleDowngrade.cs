using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic.Mechanics.Blueprints;
using Kingmaker.UnitLogic.Mechanics.Facts;
using Kingmaker.View.Covers;
using Owlcat.Runtime.Core.Logging;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Gameplay.Components;

[Serializable]
[AllowedOn(typeof(BlueprintMechanicEntityFact))]
[TypeId("310715aae5eb9334ea32b93fdc7b92c2")]
public class GridObstacleDowngrade : MechanicEntityFactComponentDelegate
{
	public LosCalculations.CoverType TargetType;

	protected override void OnActivateOrPostLoad()
	{
		if (!(base.Owner is DestructibleEntity { AllGridObstacles: var allGridObstacles } destructibleEntity))
		{
			LogChannel.Default.Error("GridObstacleDowngrade: owner is not a DestructibleEntity (got " + (base.Owner?.GetType().Name ?? "null") + ")");
		}
		else if (allGridObstacles != null)
		{
			PartGridObstacleOverrides orCreate = destructibleEntity.GetOrCreate<PartGridObstacleOverrides>();
			for (int i = 0; i < allGridObstacles.Length; i++)
			{
				orCreate.Add(base.Fact, i, TargetType);
			}
		}
	}

	protected override void OnDeactivate()
	{
		if (base.Owner is DestructibleEntity destructibleEntity)
		{
			destructibleEntity.GetOptional<PartGridObstacleOverrides>()?.Remove(base.Fact);
		}
	}
}
