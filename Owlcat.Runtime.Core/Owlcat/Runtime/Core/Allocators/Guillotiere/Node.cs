using Unity.Burst;

namespace Owlcat.Runtime.Core.Allocators.Guillotiere;

[BurstCompile]
public struct Node
{
	public int Parent;

	public int NextSibling;

	public int PrevSibling;

	public NodeKind Kind;

	public Orientation Orientation;

	public NativeRectInt Rect;
}
