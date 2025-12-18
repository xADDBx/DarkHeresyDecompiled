namespace Kingmaker.UnitLogic.Abilities;

public struct UIMoralePredictionData
{
	public int MinDelta;

	public int MaxDelta;

	public bool Equals(UIMoralePredictionData other)
	{
		if (MinDelta == other.MinDelta)
		{
			return MaxDelta == other.MaxDelta;
		}
		return false;
	}
}
