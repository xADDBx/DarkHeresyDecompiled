using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.Experimental.Collections;

[StructLayout(LayoutKind.Sequential, Size = 1)]
[BurstCompile]
public struct Less : IPredicate<sbyte, sbyte>, IPredicate<byte, byte>, IPredicate<int, int>, IPredicate<uint, uint>, IPredicate<float, float>, IPredicate<double, double>
{
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Invoke(in sbyte x, in sbyte y)
	{
		return x < y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Invoke(in byte x, in byte y)
	{
		return x < y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Invoke(in int x, in int y)
	{
		return x < y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Invoke(in uint x, in uint y)
	{
		return x < y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Invoke(in float x, in float y)
	{
		return x < y;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Invoke(in double x, in double y)
	{
		return x < y;
	}

	bool IPredicate<sbyte, sbyte>.Invoke(in sbyte a, in sbyte b)
	{
		return Invoke(in a, in b);
	}

	bool IPredicate<byte, byte>.Invoke(in byte a, in byte b)
	{
		return Invoke(in a, in b);
	}

	bool IPredicate<int, int>.Invoke(in int a, in int b)
	{
		return Invoke(in a, in b);
	}

	bool IPredicate<uint, uint>.Invoke(in uint a, in uint b)
	{
		return Invoke(in a, in b);
	}

	bool IPredicate<float, float>.Invoke(in float a, in float b)
	{
		return Invoke(in a, in b);
	}

	bool IPredicate<double, double>.Invoke(in double a, in double b)
	{
		return Invoke(in a, in b);
	}
}
