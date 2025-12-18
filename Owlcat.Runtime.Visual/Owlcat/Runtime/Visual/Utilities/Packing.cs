using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.Utilities;

public static class Packing
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint BitFieldExtract(uint data, int offset, int numBits)
	{
		uint num = (uint)((1 << numBits) - 1);
		return (data >> offset) & num;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float PackInt(uint i, int numBits)
	{
		uint num = (uint)((1 << numBits) - 1);
		return math.saturate((float)i * math.rcp(num));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint UnpackInt(float f, int numBits)
	{
		uint num = (uint)((1 << numBits) - 1);
		return (uint)((double)(f * (float)num) + 0.5);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float PackByte(uint i)
	{
		return PackInt(i, 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint UnpackByte(float f)
	{
		return UnpackInt(f, 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float PackShort(uint i)
	{
		return PackInt(i, 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static uint UnpackShort(float f)
	{
		return UnpackInt(f, 16);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float PackShortLo(uint i)
	{
		return PackInt(BitFieldExtract(i, 0, 8), 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float PackShortHi(uint i)
	{
		return PackInt(BitFieldExtract(i, 8, 8), 8);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static float2 PackFloatToR8G8(float f)
	{
		uint i = UnpackShort(f);
		return new float2(PackShortLo(i), PackShortHi(i));
	}

	public static float UnpackFloatFromR8G8(float2 f)
	{
		uint num = UnpackByte(f.x);
		return PackShort((UnpackByte(f.y) << 8) + num);
	}
}
