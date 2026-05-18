using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public struct ColliderDescriptorSoASlice
{
	public NativeSlice<Aabb> PrevAabb;

	public NativeSlice<Aabb> Aabb;

	public NativeSlice<AffineTransform> PrevTransform;

	public NativeSlice<ColliderShape> Shape;

	public NativeSlice<AffineTransform> Transform;

	public NativeSlice<int> Layer;
}
