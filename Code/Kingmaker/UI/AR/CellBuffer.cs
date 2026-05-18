using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal readonly struct CellBuffer
{
	private readonly NativeArray<CellUnion> m_CellArray;

	private readonly NativeArray<uint> m_CellAreaMaskArray;

	public int Length
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_CellArray.Length;
		}
	}

	public CellBuffer(NativeArray<CellUnion> cellArray, NativeArray<uint> cellAreaMaskArray)
	{
		m_CellArray = cellArray;
		m_CellAreaMaskArray = cellAreaMaskArray;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Cell GetCell(int index)
	{
		return m_CellArray[index].cell;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public uint GetAreaMask(int index)
	{
		return m_CellAreaMaskArray[index];
	}
}
