using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class BodyBoneSoA : StructureOfArrays<BodyBone>
{
	public NativeArray<int> ParticleIndex;

	public NativeArray<float4x4> Bindpose;

	public NativeArray<float4x4> SimulatedBindpose;

	public NativeArray<int> ParentIndex;

	public NativeArray<float4x4> Bonepose;

	public override BodyBone this[int index]
	{
		get
		{
			BodyBone result = default(BodyBone);
			result.ParticleIndex = ParticleIndex[index];
			result.Bindpose = Bindpose[index];
			result.SimulatedBindpose = SimulatedBindpose[index];
			result.ParentIndex = ParentIndex[index];
			result.Bonepose = Bonepose[index];
			return result;
		}
		set
		{
			ParticleIndex[index] = value.ParticleIndex;
			Bindpose[index] = value.Bindpose;
			SimulatedBindpose[index] = value.SimulatedBindpose;
			ParentIndex[index] = value.ParentIndex;
			Bonepose[index] = value.Bonepose;
		}
	}

	public BodyBoneSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<float4x4>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<float4x4>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		ParticleIndex = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Bindpose = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		SimulatedBindpose = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		ParentIndex = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Bonepose = new NativeArray<float4x4>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		BodyBoneSoA bodyBoneSoA = (BodyBoneSoA)dst;
		NativeArray<int>.Copy(ParticleIndex, offset, bodyBoneSoA.ParticleIndex, dstOffset, length);
		NativeArray<float4x4>.Copy(Bindpose, offset, bodyBoneSoA.Bindpose, dstOffset, length);
		NativeArray<float4x4>.Copy(SimulatedBindpose, offset, bodyBoneSoA.SimulatedBindpose, dstOffset, length);
		NativeArray<int>.Copy(ParentIndex, offset, bodyBoneSoA.ParentIndex, dstOffset, length);
		NativeArray<float4x4>.Copy(Bonepose, offset, bodyBoneSoA.Bonepose, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		ParticleIndex.Dispose();
		Bindpose.Dispose();
		SimulatedBindpose.Dispose();
		ParentIndex.Dispose();
		Bonepose.Dispose();
	}

	public BodyBoneSoASlice GetSlice(int offset, int count)
	{
		BodyBoneSoASlice result = default(BodyBoneSoASlice);
		result.ParticleIndex = new NativeSlice<int>(ParticleIndex, offset, count);
		result.Bindpose = new NativeSlice<float4x4>(Bindpose, offset, count);
		result.SimulatedBindpose = new NativeSlice<float4x4>(SimulatedBindpose, offset, count);
		result.ParentIndex = new NativeSlice<int>(ParentIndex, offset, count);
		result.Bonepose = new NativeSlice<float4x4>(Bonepose, offset, count);
		return result;
	}
}
