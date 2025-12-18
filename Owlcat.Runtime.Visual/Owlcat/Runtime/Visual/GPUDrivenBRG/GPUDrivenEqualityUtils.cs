using System;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

internal static class GPUDrivenEqualityUtils
{
	public static bool AllStringsAreEqual(string[] array1, string[] array2)
	{
		if (array1 == null || array2 == null)
		{
			return array1 == array2;
		}
		if (array1.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (array1[i] != array2[i])
			{
				return false;
			}
		}
		return true;
	}

	public static bool AllItemsAreEqual<T>(T[] array1, T[] array2) where T : struct, IEquatable<T>
	{
		if (array1 == null || array2 == null)
		{
			return array1 == array2;
		}
		if (array1.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (!array1[i].Equals(array2[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static int GetHashCode(string[] array)
	{
		if (array == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			num = HashCode.Combine(num, array[i]?.GetHashCode() ?? 0);
		}
		return num;
	}

	public static int GetHashCode<T>(T[] array) where T : struct, IEquatable<T>
	{
		if (array == null)
		{
			return 0;
		}
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			T val = array[i];
			num = HashCode.Combine(num, val.GetHashCode());
		}
		return num;
	}
}
