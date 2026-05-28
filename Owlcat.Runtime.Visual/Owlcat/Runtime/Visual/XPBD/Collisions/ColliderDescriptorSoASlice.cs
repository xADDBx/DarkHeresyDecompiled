using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public struct ColliderDescriptorSoASlice
{
	public NativeSlice<int> Layer;

	public NativeSlice<Aabb> Aabb;

	public NativeSlice<AffineTransform> PrevTransform;

	public NativeSlice<AffineTransform> Transform;

	public NativeSlice<Aabb> PrevAabb;

	public NativeSlice<ColliderShape> Shape;
}
