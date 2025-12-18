using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Burst;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

[BurstCompile]
public struct ColliderDescriptor
{
	public ColliderShape Shape;

	public Aabb Aabb;

	public Aabb PrevAabb;

	public AffineTransform Transform;

	public AffineTransform PrevTransform;

	public int Layer;
}
