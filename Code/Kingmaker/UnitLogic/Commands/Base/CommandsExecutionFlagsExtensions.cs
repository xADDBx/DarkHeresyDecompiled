namespace Kingmaker.UnitLogic.Commands.Base;

public static class CommandsExecutionFlagsExtensions
{
	public static bool Has(this CommandPreprocessingFlags value, CommandPreprocessingFlags flag)
	{
		return (value & flag) != 0;
	}
}
