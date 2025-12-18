namespace Kingmaker.Utility.DotNetExtensions;

public static class Static
{
	public static bool NotNull<T>(T value) where T : class
	{
		return value != null;
	}
}
