using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal readonly struct OutlineTraceResult
{
	public readonly bool outer;

	public readonly float3 startPosition;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public OutlineTraceResult(bool outer, float3 startPosition)
	{
		this.outer = outer;
		this.startPosition = startPosition;
	}
}
