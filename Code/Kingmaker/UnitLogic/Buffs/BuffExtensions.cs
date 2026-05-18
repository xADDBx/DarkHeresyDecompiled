using Code.Enums;
using Kingmaker.Blueprints;
using Kingmaker.Designers.Mechanics.Facts.HitChance;
using Kingmaker.UnitLogic.Buffs.Components;
using Kingmaker.Utility.CodeTimer;

namespace Kingmaker.UnitLogic.Buffs;

public static class BuffExtensions
{
	public static bool HasNegativeHitChanceModifier(this Buff buff)
	{
		using (ProfileScope.New("BuffExtensions.HasNegativeHitChanceModifier"))
		{
			foreach (HitChanceModifier component in buff.Blueprint.GetComponents<HitChanceModifier>())
			{
				if (component.HitChance.Calculate(buff.Context) < 0)
				{
					return true;
				}
			}
			return false;
		}
	}

	public static bool IsDoT(this Buff buff, out DOT dotType)
	{
		DOTLogic dOTLogic = buff?.Blueprint?.GetComponent<DOTLogic>();
		dotType = dOTLogic?.Type ?? DOT.Bleeding;
		return dOTLogic != null;
	}

	public static bool IsCriticalEffect(this Buff buff)
	{
		return (buff?.Blueprint?.IsCriticalEffect).GetValueOrDefault();
	}
}
