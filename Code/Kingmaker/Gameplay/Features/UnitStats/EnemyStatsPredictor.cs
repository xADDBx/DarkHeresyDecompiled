using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Classes.Experience;
using Kingmaker.Blueprints.Items.Weapons;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.EntitySystem.Stats.Base;
using Kingmaker.Gameplay.Features.Experience;
using Kingmaker.Settings;
using Kingmaker.Settings.Difficulty;

namespace Kingmaker.Gameplay.Features.UnitStats;

public static class EnemyStatsPredictor
{
	public readonly struct HpPrediction
	{
		public readonly int FinalHp;

		public readonly int StatBaseHp;

		public readonly int Toughness;

		public readonly int BaseWoundsValue;

		public readonly float DurabilityFactor;

		public readonly float LowCRPenalty;

		public readonly float Hardness;

		public readonly float ArmyFactor;

		public readonly float UnitFactor;

		public readonly bool IsTough;

		public readonly bool IsFragile;

		public HpPrediction(int finalHp, int statBaseHp, int toughness, int baseWoundsValue, float durabilityFactor, float lowCRPenalty, float hardness, float armyFactor, float unitFactor, bool isTough, bool isFragile)
		{
			FinalHp = finalHp;
			StatBaseHp = statBaseHp;
			Toughness = toughness;
			BaseWoundsValue = baseWoundsValue;
			DurabilityFactor = durabilityFactor;
			LowCRPenalty = lowCRPenalty;
			Hardness = hardness;
			ArmyFactor = armyFactor;
			UnitFactor = unitFactor;
			IsTough = isTough;
			IsFragile = isFragile;
		}
	}

	public readonly struct DamagePrediction
	{
		public readonly int Min;

		public readonly int Max;

		public readonly int WeaponMin;

		public readonly int WeaponMax;

		public readonly float Veterancy;

		public readonly int EnemyDamageModifierPct;

		public DamagePrediction(int min, int max, int weaponMin, int weaponMax, float veterancy, int enemyDamageModifierPct)
		{
			Min = min;
			Max = max;
			WeaponMin = weaponMin;
			WeaponMax = weaponMax;
			Veterancy = veterancy;
			EnemyDamageModifierPct = enemyDamageModifierPct;
		}
	}

	public static HpPrediction PredictMaxHp(BlueprintUnit blueprint, EnemyDifficultyOption tier, int cr)
	{
		BlueprintUnitStatsRoot unitStatsRoot = ConfigRoot.Instance.UnitStatsRoot;
		int unmodifiedWoundsBaseValue = unitStatsRoot.GetUnmodifiedWoundsBaseValue(blueprint.DifficultyType);
		bool flag = ContainsSubtype(unitStatsRoot.ToughSubtypes, blueprint.Subtype);
		bool num = ContainsSubtype(unitStatsRoot.FragileSubtypes, blueprint.Subtype);
		bool flag2 = blueprint.Body.ItemEquipmentHandSettings.Contains((BlueprintItemWeapon w) => w.IsMelee);
		bool flag3 = blueprint.Subtype == UnitSubtype.Default && (blueprint.DifficultyType == UnitDifficultyType.Common || blueprint.DifficultyType == UnitDifficultyType.Swarm) && !flag2;
		bool flag4 = num || flag3;
		float num2 = (flag ? (1f + (float)unitStatsRoot.ToughUnitHitPointsModifier / 100f) : (flag4 ? (1f + (float)unitStatsRoot.FragileUnitHitPointsModifier / 100f) : 1f));
		int num3 = blueprint.Army?.StatModifiers.HitPoints ?? 0;
		int hitPoints = blueprint.StatModifiers.HitPoints;
		float num4 = 1f + (float)num3 / 100f;
		float num5 = 1f + (float)hitPoints / 100f;
		float durabilityFactor = DifficultyUtils.GetDurabilityFactor(tier, cr);
		float num6 = Math.Min(1f, 1f - 0.1f * (float)(8 - cr));
		int num7 = (int)Math.Round((float)unmodifiedWoundsBaseValue * durabilityFactor * num6 * num2 * num4 * num5);
		int num8 = ReadToughness(blueprint, tier, cr);
		return new HpPrediction(num7 + num7 * num8 / 100, num7, num8, unmodifiedWoundsBaseValue, durabilityFactor, num6, num2, num4, num5, flag, flag4);
	}

	public static DamagePrediction PredictWeaponDamage(BlueprintItemWeapon weapon, EnemyDifficultyOption tier, int cr)
	{
		int num = weapon?.GetDamageMin() ?? 0;
		int num2 = weapon?.GetDamageMax() ?? 0;
		float damageFactor = DifficultyUtils.GetDamageFactor(tier, cr);
		int value = SettingsRoot.Difficulty.EnemyDamageModifier.GetValue();
		float num3 = 1f + (float)value / 100f;
		int min = (int)Math.Round((float)num * damageFactor * num3);
		int max = (int)Math.Round((float)num2 * damageFactor * num3);
		return new DamagePrediction(min, max, num, num2, damageFactor, value);
	}

	public static (int min, int max) ScaleBaselineDamage(int baselineMin, int baselineMax, EnemyDifficultyOption tier, int cr, EnemyDifficultyOption baselineTier, int baselineCr)
	{
		float damageFactor = DifficultyUtils.GetDamageFactor(tier, cr);
		float damageFactor2 = DifficultyUtils.GetDamageFactor(baselineTier, baselineCr);
		float num = damageFactor / damageFactor2;
		return (min: (int)Math.Round((float)baselineMin * num), max: (int)Math.Round((float)baselineMax * num));
	}

	private static bool ContainsSubtype(UnitSubtype[] subtypes, UnitSubtype target)
	{
		if (subtypes == null)
		{
			return false;
		}
		for (int i = 0; i < subtypes.Length; i++)
		{
			if (subtypes[i] == target)
			{
				return true;
			}
		}
		return false;
	}

	private static int ReadToughness(BlueprintUnit blueprint, EnemyDifficultyOption tier, int cr)
	{
		return UnitBaseStats.Get(blueprint, tier, cr)[StatType.Toughness].Value;
	}
}
