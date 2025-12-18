using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Owlcat.Runtime.Core.Allocators;

[BurstCompile]
public struct NativeRectInt
{
	private int4 m_Value;

	public int4 Value
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Value;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value = value;
		}
	}

	public int x
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Value.x;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.x = value;
		}
	}

	public int y
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Value.y;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.y = value;
		}
	}

	public int width
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Value.z;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.z = value;
		}
	}

	public int height
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Value.w;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.w = value;
		}
	}

	public int xMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return math.max(m_Value.x, m_Value.x + m_Value.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.z = value - m_Value.x;
		}
	}

	public int yMax
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return math.max(m_Value.y, m_Value.y + m_Value.w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.w = value - m_Value.y;
		}
	}

	public int xMin
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return math.min(m_Value.x, m_Value.x + m_Value.z);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			int num = xMax;
			m_Value.x = value;
			m_Value.z = num - m_Value.x;
		}
	}

	public int yMin
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return math.min(m_Value.y, m_Value.y + m_Value.w);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			int num = yMax;
			m_Value.y = value;
			m_Value.w = num - m_Value.y;
		}
	}

	public float2 center
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new float2((float)x + (float)m_Value.z / 2f, (float)y + (float)m_Value.w / 2f);
		}
	}

	public int2 min
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new int2(m_Value.x, m_Value.y);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.xy = value;
		}
	}

	public int2 max
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return new int2(xMax, yMax);
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			xMax = value.x;
			yMax = value.y;
		}
	}

	public int2 position
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Value.xy;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.xy = value;
		}
	}

	public int2 size
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Value.zw;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			m_Value.zw = value;
		}
	}

	public int area => m_Value.z * m_Value.w;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeRectInt(int x, int y, int width, int height)
	{
		m_Value = new int4(x, y, width, height);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeRectInt(int2 position, int2 size)
	{
		m_Value = new int4(position, size);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void ClampToBounds(in NativeRectInt bounds)
	{
		position = new int2(math.max(math.min(bounds.xMax, position.x), bounds.xMin), math.max(math.min(bounds.yMax, position.y), bounds.yMin));
		size = new int2(math.min(bounds.xMax - position.x, size.x), math.min(bounds.yMax - position.y, size.y));
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void SetMinMax(in int2 minPosition, in int2 maxPosition)
	{
		min = minPosition;
		max = maxPosition;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Contains(int2 position)
	{
		if (position.x >= xMin && position.y >= yMin && position.x < xMax)
		{
			return position.y < yMax;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Overlaps(in NativeRectInt other)
	{
		if (other.xMin < xMax && other.xMax > xMin && other.yMin < yMax)
		{
			return other.yMax > yMin;
		}
		return false;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString()
	{
		return ToString(null, null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(string format)
	{
		return ToString(format, null);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public string ToString(string format, IFormatProvider formatProvider)
	{
		if (formatProvider == null)
		{
			formatProvider = CultureInfo.InvariantCulture.NumberFormat;
		}
		return string.Format(CultureInfo.InvariantCulture.NumberFormat, "(x:{0}, y:{1}, width:{2}, height:{3})", x.ToString(format, formatProvider), y.ToString(format, formatProvider), width.ToString(format, formatProvider), height.ToString(format, formatProvider));
	}
}
