using Kingmaker.Utility.Attributes;
using Kingmaker.View.Covers;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Gameplay.CoverPrototypeMechanics;

public class CoverPrototype : MonoBehaviour
{
	[Header("LoS Settings")]
	[Range(-1f, 1f)]
	public float CustomLosValue;

	public bool UseCustomLosForUnitsInCover;

	[Range(-1f, 1f)]
	[ShowIf("UseCustomLosForUnitsInCover")]
	public float CustomLosValueInCover = 1f;

	[Header("Cover Settings")]
	[Range(1f, 45f)]
	public int CoverEfficiencyAngle = 20;

	[FormerlySerializedAs("UseDiminishedCoverEfficiencyAngle")]
	public bool EnableDiminishedCoverEfficiencyAngle;

	[Range(1f, 180f)]
	[ShowIf("EnableDiminishedCoverEfficiencyAngle")]
	public int DiminishedCoverEfficiencyAngle = 180;

	public bool EnableDiagonalCovers = true;

	[Header("Reduce Cover by Height Difference Settings")]
	public LosCoverPrototype.ReduceCoverEfficiencyByHeightType ReduceCoverEfficiencyByHeight = LosCoverPrototype.ReduceCoverEfficiencyByHeightType.HeightDifference;

	[ShowIf("IsReduceCoverByHeightDiff")]
	public float MinHeightDifference = 2f;

	[Range(1f, 90f)]
	[ShowIf("IsReduceCoverByVerticalAngle")]
	public int MinVerticalAngle = 90;

	[Header("Gameplay Settings")]
	public bool DisableStepOutForPlayer = true;

	public bool DisableStepOutForNPC = true;

	public bool DisableEndTurnComponentForPlayer;

	public bool DisableEndTurnComponentForNPC;

	[ShowIf("DisableEndTurnComponentForNPC")]
	public bool EnableDirtyAi;

	private bool IsReduceCoverByHeightDiff => ReduceCoverEfficiencyByHeight == LosCoverPrototype.ReduceCoverEfficiencyByHeightType.HeightDifference;

	private bool IsReduceCoverByVerticalAngle => ReduceCoverEfficiencyByHeight == LosCoverPrototype.ReduceCoverEfficiencyByHeightType.Angle;

	private void Update()
	{
		CustomLosValueInCover = (UseCustomLosForUnitsInCover ? Mathf.Min(CustomLosValueInCover, CustomLosValue) : CustomLosValue);
		DiminishedCoverEfficiencyAngle = (EnableDiminishedCoverEfficiencyAngle ? Mathf.Max(DiminishedCoverEfficiencyAngle, CoverEfficiencyAngle) : CoverEfficiencyAngle);
		LosCoverPrototype.LosBlockerRadius = CustomLosValue;
		LosCoverPrototype.LosBlockerRadiusInCover = CustomLosValueInCover;
		LosCoverPrototype.CoverEfficiencyAngle = CoverEfficiencyAngle;
		LosCoverPrototype.DiminishedCoverEfficiencyAngle = DiminishedCoverEfficiencyAngle;
		LosCoverPrototype.EnableDiagonalCovers = EnableDiagonalCovers;
		LosCoverPrototype.ReduceCoverEfficiencyByHeight = ReduceCoverEfficiencyByHeight;
		LosCoverPrototype.ReduceCoverEfficiencyHeightDifference = MinHeightDifference;
		LosCoverPrototype.ReduceCoverEfficiencyAngle = MinVerticalAngle;
		LosCoverPrototype.DisableStepOutForPlayer = DisableStepOutForPlayer;
		LosCoverPrototype.DisableStepOutForNPC = DisableStepOutForNPC;
		LosCoverPrototype.DisableEndTurnComponentForPlayer = DisableEndTurnComponentForPlayer;
		LosCoverPrototype.DisableEndTurnComponentForNPC = DisableEndTurnComponentForNPC;
		LosCoverPrototype.EnableDirtyAi = EnableDirtyAi;
	}
}
