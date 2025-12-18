using System;

namespace Kingmaker.View.Covers;

public static class LosCoverPrototype
{
	public enum ReduceCoverEfficiencyByHeightType
	{
		AsInRogueTrader,
		HeightDifference,
		Angle
	}

	public static float LosBlockerRadius = 1f;

	public static float LosBlockerRadiusInCover = 1f;

	public static int CoverEfficiencyAngle = 180;

	public static int DiminishedCoverEfficiencyAngle = 180;

	public static ReduceCoverEfficiencyByHeightType ReduceCoverEfficiencyByHeight = ReduceCoverEfficiencyByHeightType.AsInRogueTrader;

	public static float ReduceCoverEfficiencyHeightDifference = 10000f;

	public static int ReduceCoverEfficiencyAngle = 90;

	public static bool EnableDiagonalCovers = false;

	public static bool DisableStepOutForPlayer = false;

	public static bool DisableStepOutForNPC = false;

	public static bool DisableEndTurnComponentForPlayer = false;

	public static bool DisableEndTurnComponentForNPC = false;

	public static bool EnableDirtyAi = false;

	public static float CoverHalfAngleInRad => (float)CoverEfficiencyAngle * MathF.PI * 2f / 360f;

	public static float DiminishedCoverHalfAngleInRad => (float)DiminishedCoverEfficiencyAngle * MathF.PI * 2f / 360f;

	public static float ReduceCoverEfficiencyAngleInRad => (float)ReduceCoverEfficiencyAngle * MathF.PI * 2f / 180f;

	public static void Reset()
	{
		LosBlockerRadius = 1f;
		LosBlockerRadiusInCover = 1f;
		CoverEfficiencyAngle = 180;
		DiminishedCoverEfficiencyAngle = 180;
		ReduceCoverEfficiencyByHeight = ReduceCoverEfficiencyByHeightType.AsInRogueTrader;
		ReduceCoverEfficiencyHeightDifference = 10000f;
		ReduceCoverEfficiencyAngle = 90;
		EnableDiagonalCovers = false;
	}
}
