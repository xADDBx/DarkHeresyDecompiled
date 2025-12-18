using System;
using System.Runtime.InteropServices;

namespace Owlcat.Runtime.Visual.VirtualTerrain.Streaming;

[Serializable]
[StructLayout(LayoutKind.Explicit)]
public struct TerrainLayerGuid : IEquatable<TerrainLayerGuid>
{
	[FieldOffset(0)]
	public uint Value0;

	[FieldOffset(4)]
	public uint Value1;

	[FieldOffset(8)]
	public uint Value2;

	[FieldOffset(12)]
	public uint Value3;

	public TerrainLayerGuid(uint value0, uint value1, uint value2, uint value3)
	{
		Value0 = value0;
		Value1 = value1;
		Value2 = value2;
		Value3 = value3;
	}

	public bool Equals(TerrainLayerGuid other)
	{
		if (Value0 == other.Value0 && Value1 == other.Value1 && Value2 == other.Value2)
		{
			return Value3 == other.Value3;
		}
		return false;
	}

	public override bool Equals(object obj)
	{
		if (obj is TerrainLayerGuid other)
		{
			return Equals(other);
		}
		return false;
	}

	public override int GetHashCode()
	{
		return (int)((((((Value0 * 397) ^ Value1) * 397) ^ Value2) * 397) ^ Value3);
	}

	public static bool operator ==(TerrainLayerGuid left, TerrainLayerGuid right)
	{
		return left.Equals(right);
	}

	public static bool operator !=(TerrainLayerGuid left, TerrainLayerGuid right)
	{
		return !left.Equals(right);
	}
}
