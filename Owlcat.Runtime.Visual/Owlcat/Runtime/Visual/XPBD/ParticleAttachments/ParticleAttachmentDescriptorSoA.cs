using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class ParticleAttachmentDescriptorSoA : StructureOfArrays<ParticleAttachmentDescriptor>
{
	public NativeArray<int2> BodyParticlesRange;

	public NativeArray<int2> ParticleDataRange;

	public NativeArray<float4x4> LocalToWorld;

	public override ParticleAttachmentDescriptor this[int index]
	{
		get
		{
			ParticleAttachmentDescriptor result = default(ParticleAttachmentDescriptor);
			result.BodyParticlesRange = BodyParticlesRange[index];
			result.ParticleDataRange = ParticleDataRange[index];
			result.LocalToWorld = LocalToWorld[index];
			return result;
		}
		set
		{
			BodyParticlesRange[index] = value.BodyParticlesRange;
			ParticleDataRange[index] = value.ParticleDataRange;
			LocalToWorld[index] = value.LocalToWorld;
		}
	}

	public ParticleAttachmentDescriptorSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<int2>();
		num += Marshal.SizeOf<float4x4>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		BodyParticlesRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ParticleDataRange = new NativeArray<int2>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		LocalToWorld = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ParticleAttachmentDescriptorSoA particleAttachmentDescriptorSoA = (ParticleAttachmentDescriptorSoA)dst;
		NativeArray<int2>.Copy(BodyParticlesRange, offset, particleAttachmentDescriptorSoA.BodyParticlesRange, dstOffset, length);
		NativeArray<int2>.Copy(ParticleDataRange, offset, particleAttachmentDescriptorSoA.ParticleDataRange, dstOffset, length);
		NativeArray<float4x4>.Copy(LocalToWorld, offset, particleAttachmentDescriptorSoA.LocalToWorld, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		BodyParticlesRange.Dispose();
		ParticleDataRange.Dispose();
		LocalToWorld.Dispose();
	}

	public ParticleAttachmentDescriptorSoASlice GetSlice(int offset, int count)
	{
		ParticleAttachmentDescriptorSoASlice result = default(ParticleAttachmentDescriptorSoASlice);
		result.BodyParticlesRange = new NativeSlice<int2>(BodyParticlesRange, offset, count);
		result.ParticleDataRange = new NativeSlice<int2>(ParticleDataRange, offset, count);
		result.LocalToWorld = new NativeSlice<float4x4>(LocalToWorld, offset, count);
		return result;
	}
}
