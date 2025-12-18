namespace Kingmaker.UnitLogic.Abilities;

public struct UIHitChancePredictionData
{
	public float InitialHitChance;

	public float CoverChance;

	public float HitWithAvoidanceChance;

	public float CriticalEffectsAvoidanceChance;

	public float DefenceChance;

	public float OverpenetraionChance;

	public bool CasterHasCriticalEffects;

	public bool HitAlways;

	public bool Equals(UIHitChancePredictionData other)
	{
		if (object.Equals(InitialHitChance, other.InitialHitChance) && object.Equals(CoverChance, other.CoverChance) && object.Equals(HitWithAvoidanceChance, other.HitWithAvoidanceChance) && object.Equals(CriticalEffectsAvoidanceChance, other.CriticalEffectsAvoidanceChance) && object.Equals(DefenceChance, other.DefenceChance) && object.Equals(OverpenetraionChance, other.OverpenetraionChance) && object.Equals(CasterHasCriticalEffects, other.CasterHasCriticalEffects))
		{
			return object.Equals(HitAlways, other.HitAlways);
		}
		return false;
	}
}
