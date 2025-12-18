namespace Owlcat.Runtime.Core.Allocators.OffsetAllocator;

public static class SmallFloat
{
	public const uint kMantissaBits = 3u;

	public const uint kMantissaValue = 8u;

	public const uint kMantissaMask = 7u;

	public static uint UintToFloatRoundUp(uint size)
	{
		uint num = 0u;
		uint num2;
		if (size < 8)
		{
			num2 = size;
		}
		else
		{
			uint num3 = Shared.lzcnt_nonzero(size);
			uint num4 = 31 - num3 - 3;
			num = num4 + 1;
			num2 = (size >> (int)num4) & 7u;
			uint num5 = (uint)((1 << (int)num4) - 1);
			if ((size & num5) != 0)
			{
				num2++;
			}
		}
		return (num << 3) + num2;
	}

	public static uint UintToFloatRoundDown(uint size)
	{
		uint num = 0u;
		uint num2;
		if (size < 8)
		{
			num2 = size;
		}
		else
		{
			uint num3 = Shared.lzcnt_nonzero(size);
			uint num4 = 31 - num3 - 3;
			num = num4 + 1;
			num2 = (size >> (int)num4) & 7u;
		}
		return (num << 3) | num2;
	}

	public static uint FloatToUint(uint floatValue)
	{
		uint num = floatValue >> 3;
		uint num2 = floatValue & 7u;
		if (num == 0)
		{
			return num2;
		}
		return (num2 | 8) << (int)(num - 1);
	}
}
