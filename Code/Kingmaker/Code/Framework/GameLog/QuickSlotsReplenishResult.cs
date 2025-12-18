using System.Collections.Generic;
using Kingmaker.Blueprints.Items;
using Kingmaker.EntitySystem.Entities;

namespace Kingmaker.Code.Framework.GameLog;

public struct QuickSlotsReplenishResult
{
	public IReadOnlyDictionary<MechanicEntity, List<BlueprintItem>> ReplenishSuccess;

	public IReadOnlyDictionary<MechanicEntity, List<BlueprintItem>> ReplenishFailure;
}
