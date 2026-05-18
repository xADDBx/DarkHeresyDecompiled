using System.Collections.Generic;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.Predictions;

public struct UIBuffsPredictionData
{
	public IReadOnlyList<(BlueprintBuff buff, int applyChance)> PredictedBuffs;
}
