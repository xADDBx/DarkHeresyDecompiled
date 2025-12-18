namespace Kingmaker.UnitLogic.Abilities;

public struct MoralePredictionData
{
	public int MoraleDelta;

	public static MoralePredictionData operator +(MoralePredictionData lhs, MoralePredictionData rhs)
	{
		MoralePredictionData result = default(MoralePredictionData);
		result.MoraleDelta = lhs.MoraleDelta + rhs.MoraleDelta;
		return result;
	}
}
