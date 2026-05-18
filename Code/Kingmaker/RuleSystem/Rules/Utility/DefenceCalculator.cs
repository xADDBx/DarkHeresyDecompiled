using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Framework.Mechanics.Actor;

namespace Kingmaker.RuleSystem.Rules.Utility;

public static class DefenceCalculator
{
	public static int GetMaxDefenceCap(this MechanicEntity entity)
	{
		return ((entity as BaseUnitEntity)?.Body.Armor.MaybeArmor?.Blueprint.MaxDefence).GetValueOrDefault();
	}

	public static int ApplyMaxDefenceCap(this MechanicEntity entity, int rawDefence)
	{
		int maxDefenceCap = entity.GetMaxDefenceCap();
		if (maxDefenceCap <= 0 || rawDefence <= maxDefenceCap)
		{
			return rawDefence;
		}
		return maxDefenceCap;
	}

	public static int GetEffectiveDefence(this MechanicEntity entity, StatContext ctx = default(StatContext))
	{
		return entity.ApplyMaxDefenceCap(entity.Actor.GetStat(StatType.Defence, null, ctx, "GetEffectiveDefence").ModifiedValue);
	}
}
