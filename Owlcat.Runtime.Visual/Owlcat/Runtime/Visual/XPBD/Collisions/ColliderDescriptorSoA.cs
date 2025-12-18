using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public class ColliderDescriptorSoA : StructureOfArrays<ColliderDescriptor>
{
	public NativeArray<AffineTransform> PrevTransform;

	public NativeArray<ColliderShape> Shape;

	public NativeArray<Aabb> Aabb;

	public NativeArray<Aabb> PrevAabb;

	public NativeArray<int> Layer;

	public NativeArray<AffineTransform> Transform;

	public override ColliderDescriptor this[int index]
	{
		get
		{
			ColliderDescriptor result = default(ColliderDescriptor);
			result.PrevTransform = PrevTransform[index];
			result.Shape = Shape[index];
			result.Aabb = Aabb[index];
			result.PrevAabb = PrevAabb[index];
			result.Layer = Layer[index];
			result.Transform = Transform[index];
			return result;
		}
		set
		{
			PrevTransform[index] = value.PrevTransform;
			Shape[index] = value.Shape;
			Aabb[index] = value.Aabb;
			PrevAabb[index] = value.PrevAabb;
			Layer[index] = value.Layer;
			Transform[index] = value.Transform;
		}
	}

	public ColliderDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<AffineTransform>();
		num += Marshal.SizeOf<ColliderShape>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<AffineTransform>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		PrevTransform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Shape = new NativeArray<ColliderShape>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Aabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevAabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Layer = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Transform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ColliderDescriptorSoA colliderDescriptorSoA = (ColliderDescriptorSoA)dst;
		NativeArray<AffineTransform>.Copy(PrevTransform, offset, colliderDescriptorSoA.PrevTransform, dstOffset, length);
		NativeArray<ColliderShape>.Copy(Shape, offset, colliderDescriptorSoA.Shape, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(Aabb, offset, colliderDescriptorSoA.Aabb, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(PrevAabb, offset, colliderDescriptorSoA.PrevAabb, dstOffset, length);
		NativeArray<int>.Copy(Layer, offset, colliderDescriptorSoA.Layer, dstOffset, length);
		NativeArray<AffineTransform>.Copy(Transform, offset, colliderDescriptorSoA.Transform, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		PrevTransform.Dispose();
		Shape.Dispose();
		Aabb.Dispose();
		PrevAabb.Dispose();
		Layer.Dispose();
		Transform.Dispose();
	}

	public ColliderDescriptorSoASlice GetSlice(int offset, int count)
	{
		ColliderDescriptorSoASlice result = default(ColliderDescriptorSoASlice);
		result.PrevTransform = new NativeSlice<AffineTransform>(PrevTransform, offset, count);
		result.Shape = new NativeSlice<ColliderShape>(Shape, offset, count);
		result.Aabb = new NativeSlice<Aabb>(Aabb, offset, count);
		result.PrevAabb = new NativeSlice<Aabb>(PrevAabb, offset, count);
		result.Layer = new NativeSlice<int>(Layer, offset, count);
		result.Transform = new NativeSlice<AffineTransform>(Transform, offset, count);
		return result;
	}
}
