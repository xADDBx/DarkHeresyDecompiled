using Unity.Burst;

namespace Owlcat.Runtime.Core.Allocators.Guillotiere;

[BurstCompile]
public struct Allocation
{
	public static readonly Allocation Empty;

	public uint Id;

	public int NodeIndex;

	public NativeRectInt Rect;

	public bool IsEmpty()
	{
		if (Rect.area <= 0)
		{
			return true;
		}
		return false;
	}
}
