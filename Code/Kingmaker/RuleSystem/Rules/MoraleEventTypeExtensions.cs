namespace Kingmaker.RuleSystem.Rules;

public static class MoraleEventTypeExtensions
{
	public static bool HasAnyFlag(this MoraleEventType value, MoraleEventType flag)
	{
		if (flag != 0 && value != (MoraleEventType)(-1))
		{
			return (value & flag) != 0;
		}
		return true;
	}
}
