namespace Kingmaker.RuleSystem.Rules;

public static class ChanceRollTypeExtensions
{
	public static bool HasAnyFlag(this ChanceRollType flags, ChanceRollType type)
	{
		return (flags & type) != 0;
	}
}
