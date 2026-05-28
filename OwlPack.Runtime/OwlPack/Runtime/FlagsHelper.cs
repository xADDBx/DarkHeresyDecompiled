using System;

namespace OwlPack.Runtime;

internal static class FlagsHelper<T> where T : Enum
{
	public static readonly bool IsFlags = typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false);
}
