namespace Owlcat.Runtime.Core.Allocators.OffsetAllocator;

public static class Shared
{
	public static uint lzcnt_nonzero(uint x)
	{
		int num = 32;
		uint num2 = x >> 16;
		if (num2 != 0)
		{
			num -= 16;
			x = num2;
		}
		num2 = x >> 8;
		if (num2 != 0)
		{
			num -= 8;
			x = num2;
		}
		num2 = x >> 4;
		if (num2 != 0)
		{
			num -= 4;
			x = num2;
		}
		num2 = x >> 2;
		if (num2 != 0)
		{
			num -= 2;
			x = num2;
		}
		if (x >> 1 != 0)
		{
			return (uint)(num - 2);
		}
		return (uint)(num - x);
	}

	public static uint tzcnt_nonzero(uint v)
	{
		if (v == 0)
		{
			return 32u;
		}
		uint num = 31u;
		uint num2 = v << 16;
		if (num2 != 0)
		{
			num -= 16;
			v = num2;
		}
		num2 = v << 8;
		if (num2 != 0)
		{
			num -= 8;
			v = num2;
		}
		num2 = v << 4;
		if (num2 != 0)
		{
			num -= 4;
			v = num2;
		}
		num2 = v << 2;
		if (num2 != 0)
		{
			num -= 2;
			v = num2;
		}
		return num - (v << 1 >> 31);
	}
}
