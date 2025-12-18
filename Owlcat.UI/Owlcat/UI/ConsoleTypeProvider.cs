namespace Owlcat.UI;

public abstract class ConsoleTypeProvider
{
	public abstract bool TryGetConsoleType(out ConsoleType type);
}
