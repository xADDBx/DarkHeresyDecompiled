using System.Linq;
using Kingmaker.Designers.Mechanics.Facts;
using Kingmaker.UnitLogic.Buffs.Blueprints;

namespace Kingmaker.Code.Gameplay.Blueprints;

public static class BodyPartCriticalEffectExtensions
{
	public static BlueprintBuff GetCriticalEffectStageBuff(this BlueprintBodyPart bodyPart, int stage)
	{
		AddFactsOnRank addFactsOnRank = bodyPart.CriticalEffect.Blueprint.ComponentsArray.OfType<AddFactsOnRank>().FirstOrDefault((AddFactsOnRank c) => c.RankValue.Value == stage);
		if (addFactsOnRank == null)
		{
			return null;
		}
		return addFactsOnRank.Facts.OfType<BlueprintBuff>().FirstOrDefault();
	}
}
