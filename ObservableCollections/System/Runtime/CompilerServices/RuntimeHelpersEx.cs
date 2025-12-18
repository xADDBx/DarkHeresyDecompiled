namespace System.Runtime.CompilerServices;

internal static class RuntimeHelpersEx
{
	internal static bool IsReferenceOrContainsReferences<T>()
	{
		return RuntimeHelpers.IsReferenceOrContainsReferences<T>();
	}
}
