using UnityEngine;

namespace Kingmaker.Predictions;

public struct UIHitChancePredictionData
{
	public float InitialHitChance;

	public float CoverChance;

	public float HitWithAvoidanceChance;

	public float CriticalEffectsAvoidanceChance;

	public float DefenceChance;

	public float OverpenetraionChance;

	public bool HitAlways;

	public bool IsAdditionalTarget;

	public bool CasterHasCriticalEffects;

	public bool TargetHasCriticalEffects;

	public bool CanTargetFromDesiredPosition;

	public bool Equals(UIHitChancePredictionData other)
	{
		if (Mathf.Approximately(InitialHitChance, other.InitialHitChance) && Mathf.Approximately(CoverChance, other.CoverChance) && Mathf.Approximately(HitWithAvoidanceChance, other.HitWithAvoidanceChance) && Mathf.Approximately(CriticalEffectsAvoidanceChance, other.CriticalEffectsAvoidanceChance) && Mathf.Approximately(DefenceChance, other.DefenceChance) && Mathf.Approximately(OverpenetraionChance, other.OverpenetraionChance) && object.Equals(HitAlways, other.HitAlways) && object.Equals(IsAdditionalTarget, other.IsAdditionalTarget) && object.Equals(CasterHasCriticalEffects, other.CasterHasCriticalEffects) && object.Equals(TargetHasCriticalEffects, other.TargetHasCriticalEffects))
		{
			return object.Equals(CanTargetFromDesiredPosition, other.CanTargetFromDesiredPosition);
		}
		return false;
	}
}
