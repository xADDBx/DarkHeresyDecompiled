using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class ParticleAttachmentDataSoA : StructureOfArrays<ParticleAttachmentData>
{
	public NativeArray<float3> PositionOffset;

	public NativeArray<int> IndexInBody;

	public override ParticleAttachmentData this[int index]
	{
		get
		{
			ParticleAttachmentData result = default(ParticleAttachmentData);
			result.PositionOffset = PositionOffset[index];
			result.IndexInBody = IndexInBody[index];
			return result;
		}
		set
		{
			PositionOffset[index] = value.PositionOffset;
			IndexInBody[index] = value.IndexInBody;
		}
	}

	public ParticleAttachmentDataSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<int>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		PositionOffset = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		IndexInBody = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ParticleAttachmentDataSoA particleAttachmentDataSoA = (ParticleAttachmentDataSoA)dst;
		NativeArray<float3>.Copy(PositionOffset, offset, particleAttachmentDataSoA.PositionOffset, dstOffset, length);
		NativeArray<int>.Copy(IndexInBody, offset, particleAttachmentDataSoA.IndexInBody, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		PositionOffset.Dispose();
		IndexInBody.Dispose();
	}

	public ParticleAttachmentDataSoASlice GetSlice(int offset, int count)
	{
		ParticleAttachmentDataSoASlice result = default(ParticleAttachmentDataSoASlice);
		result.PositionOffset = new NativeSlice<float3>(PositionOffset, offset, count);
		result.IndexInBody = new NativeSlice<int>(IndexInBody, offset, count);
		return result;
	}
}
