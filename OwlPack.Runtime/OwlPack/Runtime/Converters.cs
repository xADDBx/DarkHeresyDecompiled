using System;
using System.Runtime.CompilerServices;

namespace OwlPack.Runtime;

public static class Converters
{
	public static long WidenToLong(int v)
	{
		return v;
	}

	public static long WidenToLong(short v)
	{
		return v;
	}

	public static long WidenToLong(sbyte v)
	{
		return v;
	}

	public static ulong WidenToUlong(uint v)
	{
		return v;
	}

	public static ulong WitenToUlong(ushort v)
	{
		return v;
	}

	public static ulong WidenToUlong(byte v)
	{
		return v;
	}

	public static int WidenToInt(short v)
	{
		return v;
	}

	public static int WidenToInt(sbyte v)
	{
		return v;
	}

	public static uint WitenToUint(ushort v)
	{
		return v;
	}

	public static uint WidenToUint(byte v)
	{
		return v;
	}

	public static short WidenToShort(sbyte v)
	{
		return v;
	}

	public static ushort WidenToUshort(byte v)
	{
		return v;
	}

	public static double WidenToFloat(float v)
	{
		return v;
	}

	public static float ToFloat(long v)
	{
		return v;
	}

	public static float ToFloat(int v)
	{
		return v;
	}

	public static float ToFloat(short v)
	{
		return v;
	}

	public static float ToFloat(sbyte v)
	{
		return v;
	}

	public static float ToFloat(ulong v)
	{
		return v;
	}

	public static float ToFloat(uint v)
	{
		return v;
	}

	public static float ToFloat(ushort v)
	{
		return (int)v;
	}

	public static float ToFloat(byte v)
	{
		return (int)v;
	}

	public static double ToDouble(long v)
	{
		return v;
	}

	public static double ToDouble(int v)
	{
		return v;
	}

	public static double ToDouble(short v)
	{
		return v;
	}

	public static double ToDouble(sbyte v)
	{
		return v;
	}

	public static double ToDouble(ulong v)
	{
		return v;
	}

	public static double ToDouble(uint v)
	{
		return v;
	}

	public static double ToDouble(ushort v)
	{
		return (int)v;
	}

	public static double ToDouble(byte v)
	{
		return (int)v;
	}

	public static TNew TrySafeConvert<TNew, TOld>(TOld source) where TNew : unmanaged where TOld : unmanaged
	{
		if (Unsafe.SizeOf<TNew>() < Unsafe.SizeOf<TOld>())
		{
			throw new InvalidCastException($"Cannot convert {typeof(TOld)} to {typeof(TNew)}: target type is smaller than source type.");
		}
		if (source is float num && typeof(TNew) == typeof(double))
		{
			double source2 = num;
			return Unsafe.As<double, TNew>(ref source2);
		}
		TNew val = default(TNew);
		bool flag = ((source is sbyte || source is short || source is int || source is long) ? true : false);
		bool num2 = flag;
		flag = ((val is sbyte || val is short || val is int || val is long) ? true : false);
		bool flag2 = flag;
		if (num2 != flag2)
		{
			throw new InvalidCastException($"Cannot convert {typeof(TOld)} to {typeof(TNew)}: signednesss differs.");
		}
		long num7;
		if (!(source is sbyte b))
		{
			if (!(source is short num3))
			{
				if (!(source is int num4))
				{
					if (!(source is byte b2))
					{
						if (!(source is ushort num5))
						{
							if (!(source is uint num6))
							{
								throw new InvalidCastException($"Cannot convert {typeof(TOld)} to {typeof(TNew)}: no available conversion.");
							}
							num7 = num6;
						}
						else
						{
							num7 = num5;
						}
					}
					else
					{
						num7 = b2;
					}
				}
				else
				{
					num7 = num4;
				}
			}
			else
			{
				num7 = num3;
			}
		}
		else
		{
			num7 = b;
		}
		if (!(val is short))
		{
			if (!(val is ushort))
			{
				if (!(val is int))
				{
					if (!(val is uint))
					{
						if (!(val is long))
						{
							if (val is ulong)
							{
								ulong source3 = (ulong)num7;
								return Unsafe.As<ulong, TNew>(ref Unsafe.AsRef(in source3));
							}
							return default(TNew);
						}
						long source4 = num7;
						return Unsafe.As<long, TNew>(ref Unsafe.AsRef(in source4));
					}
					uint source5 = (uint)num7;
					return Unsafe.As<uint, TNew>(ref Unsafe.AsRef(in source5));
				}
				int source6 = (int)num7;
				return Unsafe.As<int, TNew>(ref Unsafe.AsRef(in source6));
			}
			ushort source7 = (ushort)num7;
			return Unsafe.As<ushort, TNew>(ref Unsafe.AsRef(in source7));
		}
		short source8 = (short)num7;
		return Unsafe.As<short, TNew>(ref Unsafe.AsRef(in source8));
	}
}
