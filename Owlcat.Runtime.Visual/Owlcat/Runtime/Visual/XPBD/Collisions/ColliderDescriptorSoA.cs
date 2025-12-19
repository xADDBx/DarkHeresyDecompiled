using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public class ColliderDescriptorSoA : StructureOfArrays<ColliderDescriptor>
{
	public NativeArray<int> Layer;

	public NativeArray<Aabb> PrevAabb;

	public NativeArray<AffineTransform> PrevTransform;

	public NativeArray<ColliderShape> Shape;

	public NativeArray<AffineTransform> Transform;

	public NativeArray<Aabb> Aabb;

	public override ColliderDescriptor this[int index]
	{
		get
		{
			ColliderDescriptor result = default(ColliderDescriptor);
			result.Layer = Layer[index];
			result.PrevAabb = PrevAabb[index];
			result.PrevTransform = PrevTransform[index];
			result.Shape = Shape[index];
			result.Transform = Transform[index];
			result.Aabb = Aabb[index];
			return result;
		}
		set
		{
			Layer[index] = value.Layer;
			PrevAabb[index] = value.PrevAabb;
			PrevTransform[index] = value.PrevTransform;
			Shape[index] = value.Shape;
			Transform[index] = value.Transform;
			Aabb[index] = value.Aabb;
		}
	}

	public ColliderDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<AffineTransform>();
		num += Marshal.SizeOf<ColliderShape>();
		num += Marshal.SizeOf<AffineTransform>();
		num += Marshal.SizeOf<Aabb>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Layer = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevAabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevTransform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Shape = new NativeArray<ColliderShape>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Transform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Aabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ColliderDescriptorSoA colliderDescriptorSoA = (ColliderDescriptorSoA)dst;
		NativeArray<int>.Copy(Layer, offset, colliderDescriptorSoA.Layer, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(PrevAabb, offset, colliderDescriptorSoA.PrevAabb, dstOffset, length);
		NativeArray<AffineTransform>.Copy(PrevTransform, offset, colliderDescriptorSoA.PrevTransform, dstOffset, length);
		NativeArray<ColliderShape>.Copy(Shape, offset, colliderDescriptorSoA.Shape, dstOffset, length);
		NativeArray<AffineTransform>.Copy(Transform, offset, colliderDescriptorSoA.Transform, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(Aabb, offset, colliderDescriptorSoA.Aabb, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		Layer.Dispose();
		PrevAabb.Dispose();
		PrevTransform.Dispose();
		Shape.Dispose();
		Transform.Dispose();
		Aabb.Dispose();
	}

	public ColliderDescriptorSoASlice GetSlice(int offset, int count)
	{
		ColliderDescriptorSoASlice result = default(ColliderDescriptorSoASlice);
		result.Layer = new NativeSlice<int>(Layer, offset, count);
		result.PrevAabb = new NativeSlice<Aabb>(PrevAabb, offset, count);
		result.PrevTransform = new NativeSlice<AffineTransform>(PrevTransform, offset, count);
		result.Shape = new NativeSlice<ColliderShape>(Shape, offset, count);
		result.Transform = new NativeSlice<AffineTransform>(Transform, offset, count);
		result.Aabb = new NativeSlice<Aabb>(Aabb, offset, count);
		return result;
	}
}
