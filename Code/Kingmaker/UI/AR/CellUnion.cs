using System.Runtime.InteropServices;
using Unity.Burst;

namespace Kingmaker.UI.AR;

[StructLayout(LayoutKind.Explicit)]
[BurstCompile]
public struct CellUnion
{
	[FieldOffset(0)]
	public IntermediateCell intermediateCell;

	[FieldOffset(0)]
	public Cell cell;
}
