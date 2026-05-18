using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Predictions;

namespace Kingmaker.Framework.Abilities;

public class AbilityPredictionResult
{
	public static readonly AbilityPredictionResult Empty = new AbilityPredictionResult(new Dictionary<MechanicEntity, AbilityPredictionContext.TargetPredictionData>());

	private readonly Dictionary<MechanicEntity, AbilityTargetPrediction> _targets;

	public IReadOnlyCollection<MechanicEntity> Targets => _targets.Keys;

	public AbilityPredictionResult(Dictionary<MechanicEntity, AbilityPredictionContext.TargetPredictionData> perTargetData)
	{
		_targets = new Dictionary<MechanicEntity, AbilityTargetPrediction>(perTargetData.Count);
		foreach (var (key, targetPredictionData2) in perTargetData)
		{
			_targets[key] = new AbilityTargetPrediction
			{
				Damage = ConvertDamage(targetPredictionData2),
				HitChance = targetPredictionData2.HitChance,
				Heal = ConvertHeal(targetPredictionData2),
				Morale = ConvertMorale(targetPredictionData2),
				Buffs = ConvertBuffs(targetPredictionData2)
			};
		}
	}

	public AbilityTargetPrediction GetPrediction(MechanicEntity target)
	{
		return _targets.GetValueOrDefault(target);
	}

	private static UIDamagePredictionData ConvertDamage(AbilityPredictionContext.TargetPredictionData data)
	{
		UIDamagePredictionData result = default(UIDamagePredictionData);
		result.MinDamagePerAttack = data.Damage.MinDamage;
		result.MaxDamagePerAttack = data.Damage.MaxDamage;
		result.HPDamageBonus = data.Damage.HPDamageBonus;
		result.ArmorDamageBonus = data.Damage.ArmorDamageBonus;
		result.VitalDamage = data.Damage.VitalDamage;
		return result;
	}

	private static UIHealPredictionData ConvertHeal(AbilityPredictionContext.TargetPredictionData data)
	{
		UIHealPredictionData result = default(UIHealPredictionData);
		result.Bonus = data.Heal.Bonus;
		result.MinHeal = data.Heal.MinValue;
		result.MaxHeal = data.Heal.MaxValue;
		result.HealStrategy = data.Heal.HealStrategy;
		return result;
	}

	private static UIMoralePredictionData ConvertMorale(AbilityPredictionContext.TargetPredictionData data)
	{
		UIMoralePredictionData result = default(UIMoralePredictionData);
		result.MinDelta = data.MoraleMinDelta;
		result.MaxDelta = data.MoraleMaxDelta;
		return result;
	}

	private static UIBuffsPredictionData ConvertBuffs(AbilityPredictionContext.TargetPredictionData data)
	{
		UIBuffsPredictionData result = default(UIBuffsPredictionData);
		result.PredictedBuffs = data.Buffs;
		return result;
	}
}
