namespace Owlcat.UI;

public class RewiredTypeProvider : ConsoleTypeProvider
{
	public override bool TryGetConsoleType(out ConsoleType type)
	{
		type = ConsoleType.Common;
		return false;
	}
}
