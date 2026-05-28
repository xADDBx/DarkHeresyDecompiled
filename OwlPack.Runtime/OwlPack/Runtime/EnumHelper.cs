using System;

namespace OwlPack.Runtime;

internal static class EnumHelper
{
	public static bool TryParse<T>(Type type, string value, out T? result)
	{
		try
		{
			result = (T)Enum.Parse(typeof(T), value);
			return true;
		}
		catch (ArgumentException)
		{
			result = default(T);
			return false;
		}
	}

	public static long ToInt64<T>(T value) where T : Enum
	{
		if (Enum.GetUnderlyingType(typeof(T)) == typeof(ulong))
		{
			return (long)Convert.ToUInt64(value);
		}
		return Convert.ToInt64(value);
	}

	public static T FromInt64<T>(long value) where T : Enum
	{
		return (T)Enum.ToObject(typeof(T), value);
	}
}
