using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public class ColliderDescriptorSoA : StructureOfArrays<ColliderDescriptor>
{
	public NativeArray<int> Layer;

	public NativeArray<Aabb> Aabb;

	public NativeArray<AffineTransform> PrevTransform;

	public NativeArray<AffineTransform> Transform;

	public NativeArray<Aabb> PrevAabb;

	public NativeArray<ColliderShape> Shape;

	public override ColliderDescriptor this[int index]
	{
		get
		{
			ColliderDescriptor result = default(ColliderDescriptor);
			result.Layer = Layer[index];
			result.Aabb = Aabb[index];
			result.PrevTransform = PrevTransform[index];
			result.Transform = Transform[index];
			result.PrevAabb = PrevAabb[index];
			result.Shape = Shape[index];
			return result;
		}
		set
		{
			Layer[index] = value.Layer;
			Aabb[index] = value.Aabb;
			PrevTransform[index] = value.PrevTransform;
			Transform[index] = value.Transform;
			PrevAabb[index] = value.PrevAabb;
			Shape[index] = value.Shape;
		}
	}

	public ColliderDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<AffineTransform>();
		num += Marshal.SizeOf<AffineTransform>();
		num += Marshal.SizeOf<Aabb>();
		num += Marshal.SizeOf<ColliderShape>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Layer = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Aabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevTransform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Transform = new NativeArray<AffineTransform>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevAabb = new NativeArray<Aabb>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Shape = new NativeArray<ColliderShape>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ColliderDescriptorSoA colliderDescriptorSoA = (ColliderDescriptorSoA)dst;
		NativeArray<int>.Copy(Layer, offset, colliderDescriptorSoA.Layer, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(Aabb, offset, colliderDescriptorSoA.Aabb, dstOffset, length);
		NativeArray<AffineTransform>.Copy(PrevTransform, offset, colliderDescriptorSoA.PrevTransform, dstOffset, length);
		NativeArray<AffineTransform>.Copy(Transform, offset, colliderDescriptorSoA.Transform, dstOffset, length);
		NativeArray<Owlcat.Runtime.Visual.XPBD.DataStructures.Aabb>.Copy(PrevAabb, offset, colliderDescriptorSoA.PrevAabb, dstOffset, length);
		NativeArray<ColliderShape>.Copy(Shape, offset, colliderDescriptorSoA.Shape, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		Layer.Dispose();
		Aabb.Dispose();
		PrevTransform.Dispose();
		Transform.Dispose();
		PrevAabb.Dispose();
		Shape.Dispose();
	}

	public ColliderDescriptorSoASlice GetSlice(int offset, int count)
	{
		ColliderDescriptorSoASlice result = default(ColliderDescriptorSoASlice);
		result.Layer = new NativeSlice<int>(Layer, offset, count);
		result.Aabb = new NativeSlice<Aabb>(Aabb, offset, count);
		result.PrevTransform = new NativeSlice<AffineTransform>(PrevTransform, offset, count);
		result.Transform = new NativeSlice<AffineTransform>(Transform, offset, count);
		result.PrevAabb = new NativeSlice<Aabb>(PrevAabb, offset, count);
		result.Shape = new NativeSlice<ColliderShape>(Shape, offset, count);
		return result;
	}
}
