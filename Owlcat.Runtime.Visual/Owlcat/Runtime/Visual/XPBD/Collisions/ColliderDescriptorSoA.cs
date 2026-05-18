using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public class ColliderDescriptorSoA : StructureOfArrays<ColliderDescriptor>
{
	public NativeArray<Aabb> PrevAabb;

	public NativeArray<Aabb> Aabb;

	public NativeArray<AffineTransform> PrevTransform;

	public NativeArray<ColliderShape> Shape;

	public NativeArray<AffineTransform> Transform;

	public NativeArray<int> Layer;

	public override ColliderDescriptor this[int index]
	{
		get
		{
			ColliderDescriptor result = default(ColliderDescriptor);
			result.PrevAabb = PrevAabb[index];
			result.Aabb = Aabb[index];
			result.PrevTransform = PrevTransform[index];
			result.Shape = Shape[index];
			result.Transform = Transform[index];
			result.Layer = Layer[index];
			return result;
		}
		set
		{
			PrevAabb[index] = value.PrevAabb;
			Aabb[index] = value.Aabb;
			PrevTransform[index] = value.PrevTransform;
			Shape[index] = value.Shape;
			Transform[index] = value.Transform;
			Layer[index] = value.Layer;
		}
	}

	public ColliderDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<AffineTransform>();
		num += Marshal.SizeOf<ColliderShape>();
		num += Marshal.SizeOf<AffineTransform>();
		num += Marshal.SizeOf<int>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		PrevAabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Aabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevTransform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Shape = new NativeArray<ColliderShape>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Transform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Layer = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ColliderDescriptorSoA colliderDescriptorSoA = (ColliderDescriptorSoA)dst;
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(PrevAabb, offset, colliderDescriptorSoA.PrevAabb, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(Aabb, offset, colliderDescriptorSoA.Aabb, dstOffset, length);
		NativeArray<AffineTransform>.Copy(PrevTransform, offset, colliderDescriptorSoA.PrevTransform, dstOffset, length);
		NativeArray<ColliderShape>.Copy(Shape, offset, colliderDescriptorSoA.Shape, dstOffset, length);
		NativeArray<AffineTransform>.Copy(Transform, offset, colliderDescriptorSoA.Transform, dstOffset, length);
		NativeArray<int>.Copy(Layer, offset, colliderDescriptorSoA.Layer, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		PrevAabb.Dispose();
		Aabb.Dispose();
		PrevTransform.Dispose();
		Shape.Dispose();
		Transform.Dispose();
		Layer.Dispose();
	}

	public ColliderDescriptorSoASlice GetSlice(int offset, int count)
	{
		ColliderDescriptorSoASlice result = default(ColliderDescriptorSoASlice);
		result.PrevAabb = new NativeSlice<Aabb>(PrevAabb, offset, count);
		result.Aabb = new NativeSlice<Aabb>(Aabb, offset, count);
		result.PrevTransform = new NativeSlice<AffineTransform>(PrevTransform, offset, count);
		result.Shape = new NativeSlice<ColliderShape>(Shape, offset, count);
		result.Transform = new NativeSlice<AffineTransform>(Transform, offset, count);
		result.Layer = new NativeSlice<int>(Layer, offset, count);
		return result;
	}
}
