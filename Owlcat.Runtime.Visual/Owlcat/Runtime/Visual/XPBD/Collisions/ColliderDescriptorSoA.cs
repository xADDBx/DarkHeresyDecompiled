using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public class ColliderDescriptorSoA : StructureOfArrays<ColliderDescriptor>
{
	public NativeArray<Aabb> Aabb;

	public NativeArray<AffineTransform> Transform;

	public NativeArray<Aabb> PrevAabb;

	public NativeArray<ColliderShape> Shape;

	public NativeArray<int> Layer;

	public NativeArray<AffineTransform> PrevTransform;

	public override ColliderDescriptor this[int index]
	{
		get
		{
			ColliderDescriptor result = default(ColliderDescriptor);
			result.Aabb = Aabb[index];
			result.Transform = Transform[index];
			result.PrevAabb = PrevAabb[index];
			result.Shape = Shape[index];
			result.Layer = Layer[index];
			result.PrevTransform = PrevTransform[index];
			return result;
		}
		set
		{
			Aabb[index] = value.Aabb;
			Transform[index] = value.Transform;
			PrevAabb[index] = value.PrevAabb;
			Shape[index] = value.Shape;
			Layer[index] = value.Layer;
			PrevTransform[index] = value.PrevTransform;
		}
	}

	public ColliderDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<AffineTransform>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<ColliderShape>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<AffineTransform>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Aabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Transform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevAabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Shape = new NativeArray<ColliderShape>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Layer = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevTransform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ColliderDescriptorSoA colliderDescriptorSoA = (ColliderDescriptorSoA)dst;
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(Aabb, offset, colliderDescriptorSoA.Aabb, dstOffset, length);
		NativeArray<AffineTransform>.Copy(Transform, offset, colliderDescriptorSoA.Transform, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(PrevAabb, offset, colliderDescriptorSoA.PrevAabb, dstOffset, length);
		NativeArray<ColliderShape>.Copy(Shape, offset, colliderDescriptorSoA.Shape, dstOffset, length);
		NativeArray<int>.Copy(Layer, offset, colliderDescriptorSoA.Layer, dstOffset, length);
		NativeArray<AffineTransform>.Copy(PrevTransform, offset, colliderDescriptorSoA.PrevTransform, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		Aabb.Dispose();
		Transform.Dispose();
		PrevAabb.Dispose();
		Shape.Dispose();
		Layer.Dispose();
		PrevTransform.Dispose();
	}

	public ColliderDescriptorSoASlice GetSlice(int offset, int count)
	{
		ColliderDescriptorSoASlice result = default(ColliderDescriptorSoASlice);
		result.Aabb = new NativeSlice<Aabb>(Aabb, offset, count);
		result.Transform = new NativeSlice<AffineTransform>(Transform, offset, count);
		result.PrevAabb = new NativeSlice<Aabb>(PrevAabb, offset, count);
		result.Shape = new NativeSlice<ColliderShape>(Shape, offset, count);
		result.Layer = new NativeSlice<int>(Layer, offset, count);
		result.PrevTransform = new NativeSlice<AffineTransform>(PrevTransform, offset, count);
		return result;
	}
}
