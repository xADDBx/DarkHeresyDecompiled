using System.Runtime.CompilerServices;

namespace Kingmaker.EntitySystem.Properties;

public static class BoolPropertyChecker
{
	public enum Mode
	{
		Ignore,
		True,
		False
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool Check(this Mode flag, bool state)
	{
		if (flag != 0)
		{
			return state == (flag == Mode.True);
		}
		return true;
	}
}
