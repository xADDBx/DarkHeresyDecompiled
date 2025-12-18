using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public struct ColliderDescriptorSoASlice
{
	public NativeSlice<AffineTransform> PrevTransform;

	public NativeSlice<ColliderShape> Shape;

	public NativeSlice<Aabb> Aabb;

	public NativeSlice<Aabb> PrevAabb;

	public NativeSlice<int> Layer;

	public NativeSlice<AffineTransform> Transform;
}
