using System.Runtime.CompilerServices;
using Unity.Burst;

namespace Kingmaker.UI.AR;

[BurstCompile]
internal readonly struct OutlineSplineMetaData
{
	public readonly bool outer;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public OutlineSplineMetaData(bool outer)
	{
		this.outer = outer;
	}
}
