using System;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.VirtualTexture.Materials;

[BurstCompile]
public struct MaterialStackIndices
{
	public int Count;

	public int Index0;

	public int Index1;

	public int Index2;

	public int Index3;

	public int Index4;

	public int Index5;

	public int Index6;

	public int Index7;

	public int Index8;

	public int Index9;

	public int Index10;

	public int Index11;

	public int Index12;

	public int Index13;

	public int Index14;

	public int Index15;

	public int this[int index]
	{
		get
		{
			return index switch
			{
				0 => Index0, 
				1 => Index1, 
				2 => Index2, 
				3 => Index3, 
				4 => Index4, 
				5 => Index5, 
				6 => Index6, 
				7 => Index7, 
				8 => Index8, 
				9 => Index9, 
				10 => Index10, 
				11 => Index11, 
				12 => Index12, 
				13 => Index13, 
				14 => Index14, 
				15 => Index15, 
				_ => throw new IndexOutOfRangeException(), 
			};
		}
		set
		{
			switch (index)
			{
			case 0:
				Index0 = value;
				break;
			case 1:
				Index1 = value;
				break;
			case 2:
				Index2 = value;
				break;
			case 3:
				Index3 = value;
				break;
			case 4:
				Index4 = value;
				break;
			case 5:
				Index5 = value;
				break;
			case 6:
				Index6 = value;
				break;
			case 7:
				Index7 = value;
				break;
			case 8:
				Index8 = value;
				break;
			case 9:
				Index9 = value;
				break;
			case 10:
				Index10 = value;
				break;
			case 11:
				Index11 = value;
				break;
			case 12:
				Index12 = value;
				break;
			case 13:
				Index13 = value;
				break;
			case 14:
				Index14 = value;
				break;
			case 15:
				Index15 = value;
				break;
			default:
				throw new IndexOutOfRangeException();
			}
		}
	}

	public override bool Equals(object obj)
	{
		if (obj is MaterialStackIndices materialStackIndices && Count == materialStackIndices.Count && Index0 == materialStackIndices.Index0 && Index1 == materialStackIndices.Index1 && Index2 == materialStackIndices.Index2 && Index3 == materialStackIndices.Index3 && Index4 == materialStackIndices.Index4 && Index5 == materialStackIndices.Index5 && Index6 == materialStackIndices.Index6 && Index7 == materialStackIndices.Index7 && Index8 == materialStackIndices.Index8 && Index9 == materialStackIndices.Index9 && Index10 == materialStackIndices.Index10 && Index11 == materialStackIndices.Index11 && Index12 == materialStackIndices.Index12 && Index13 == materialStackIndices.Index13 && Index14 == materialStackIndices.Index14)
		{
			return Index15 == materialStackIndices.Index15;
		}
		return false;
	}

	public override int GetHashCode()
	{
		HashCode hashCode = default(HashCode);
		hashCode.Add(Count);
		hashCode.Add(Index0);
		hashCode.Add(Index1);
		hashCode.Add(Index2);
		hashCode.Add(Index3);
		hashCode.Add(Index4);
		hashCode.Add(Index5);
		hashCode.Add(Index6);
		hashCode.Add(Index7);
		hashCode.Add(Index8);
		hashCode.Add(Index9);
		hashCode.Add(Index10);
		hashCode.Add(Index11);
		hashCode.Add(Index12);
		hashCode.Add(Index13);
		hashCode.Add(Index14);
		hashCode.Add(Index15);
		return hashCode.ToHashCode();
	}
}
