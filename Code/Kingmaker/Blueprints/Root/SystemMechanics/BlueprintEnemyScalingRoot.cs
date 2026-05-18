using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.Settings;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.SystemMechanics;

[ComponentName("Root/BlueprintEnemyScalingRoot")]
[TypeId("08e7d3542399cfa438d81bf4f4ff7ba5")]
public class BlueprintEnemyScalingRoot : BlueprintScriptableObject
{
	public const int MinCR = 1;

	public const int MaxCR = 25;

	public const int CRCount = 25;

	public float[] StoryFactors = new float[25];

	public float[] NormalFactors = new float[25];

	public float[] DaringFactors = new float[25];

	public float[] HardFactors = new float[25];

	public float[] UnfairFactors = new float[25];

	public float GetDamageFactor(EnemyDifficultyOption tier, int cr)
	{
		int num = Mathf.Clamp(cr, 1, 25) - 1;
		float[] row = GetRow(tier);
		if (row == null || num >= row.Length)
		{
			return 1f;
		}
		return row[num];
	}

	private float[] GetRow(EnemyDifficultyOption tier)
	{
		return tier switch
		{
			EnemyDifficultyOption.Story => StoryFactors, 
			EnemyDifficultyOption.Normal => NormalFactors, 
			EnemyDifficultyOption.Daring => DaringFactors, 
			EnemyDifficultyOption.Hard => HardFactors, 
			EnemyDifficultyOption.Unfair => UnfairFactors, 
			_ => throw new ArgumentOutOfRangeException("tier", tier, null), 
		};
	}
}
