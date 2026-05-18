using System;
using Kingmaker.Blueprints.Root;

namespace Kingmaker.Settings.Difficulty;

public static class DifficultyUtils
{
	private static bool _MissingRootWarned;

	public static float GetDurabilityFactor(EnemyDifficultyOption tier, int cr)
	{
		float num = Math.Max(1f, 0.75f + (float)cr * 0.13f);
		return tier switch
		{
			EnemyDifficultyOption.Story => Lerp(num, 0.5f) - 0.3f, 
			EnemyDifficultyOption.Normal => num, 
			EnemyDifficultyOption.Daring => Lerp(num, 0.75f) * Lerp(num, 0.25f), 
			EnemyDifficultyOption.Hard => Lerp(num, 0.5f) * Lerp(num, 0.3f) * Lerp(num, 0.2f), 
			EnemyDifficultyOption.Unfair => Lerp(num, 0.25f) * Lerp(num, 0.25f) * Lerp(num, 0.25f) * Lerp(num, 0.25f), 
			_ => throw new ArgumentOutOfRangeException("tier", tier, null), 
		};
		static float Lerp(float sf, float weight)
		{
			return 1f + (sf - 1f) * weight;
		}
	}

	public static float GetDamageFactor(EnemyDifficultyOption tier, int cr)
	{
		return ConfigRoot.Instance.EnemyScalingRoot?.GetDamageFactor(tier, cr) ?? GetFallbackDamageFactor();
	}

	private static float GetFallbackDamageFactor()
	{
		if (!_MissingRootWarned)
		{
			_MissingRootWarned = true;
			PFLog.Default.Warning("[DifficultyUtils] EnemyScalingRoot blueprint not loaded — using neutral 1x damage factor. Run \"Owlcat Tools/GameDesign/Balance/Import Combat Veterancy\" to populate the table.");
		}
		return 1f;
	}
}
