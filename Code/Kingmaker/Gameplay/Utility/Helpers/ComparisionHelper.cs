using System;

namespace Kingmaker.Gameplay.Utility.Helpers;

public static class ComparisionHelper
{
	public static bool Check(this ComparisionType comparision, long checkValue, long targetValue)
	{
		return comparision switch
		{
			ComparisionType.Equal => checkValue == targetValue, 
			ComparisionType.Greater => checkValue > targetValue, 
			ComparisionType.GreaterOrEqual => checkValue >= targetValue, 
			ComparisionType.Less => checkValue < targetValue, 
			ComparisionType.LessOrEqual => checkValue <= targetValue, 
			_ => throw new ArgumentOutOfRangeException("comparision", comparision, null), 
		};
	}

	public static string GetDescription(this ComparisionType comparision, object targetValue)
	{
		return comparision switch
		{
			ComparisionType.Equal => $"equal to {targetValue}", 
			ComparisionType.Greater => $"greater than {targetValue}", 
			ComparisionType.GreaterOrEqual => $"greater or equal to {targetValue}", 
			ComparisionType.Less => $"less than {targetValue}", 
			ComparisionType.LessOrEqual => $"less or equal to {targetValue}", 
			_ => throw new ArgumentOutOfRangeException("comparision", comparision, null), 
		};
	}
}
