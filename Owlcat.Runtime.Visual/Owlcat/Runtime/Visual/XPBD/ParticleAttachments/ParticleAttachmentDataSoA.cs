using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class ParticleAttachmentDataSoA : StructureOfArrays<ParticleAttachmentData>
{
	public NativeArray<int> IndexInBody;

	public NativeArray<float3> PositionOffset;

	public override ParticleAttachmentData this[int index]
	{
		get
		{
			ParticleAttachmentData result = default(ParticleAttachmentData);
			result.IndexInBody = IndexInBody[index];
			result.PositionOffset = PositionOffset[index];
			return result;
		}
		set
		{
			IndexInBody[index] = value.IndexInBody;
			PositionOffset[index] = value.PositionOffset;
		}
	}

	public ParticleAttachmentDataSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<float3>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		IndexInBody = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PositionOffset = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ParticleAttachmentDataSoA particleAttachmentDataSoA = (ParticleAttachmentDataSoA)dst;
		NativeArray<int>.Copy(IndexInBody, offset, particleAttachmentDataSoA.IndexInBody, dstOffset, length);
		NativeArray<float3>.Copy(PositionOffset, offset, particleAttachmentDataSoA.PositionOffset, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		IndexInBody.Dispose();
		PositionOffset.Dispose();
	}

	public ParticleAttachmentDataSoASlice GetSlice(int offset, int count)
	{
		ParticleAttachmentDataSoASlice result = default(ParticleAttachmentDataSoASlice);
		result.IndexInBody = new NativeSlice<int>(IndexInBody, offset, count);
		result.PositionOffset = new NativeSlice<float3>(PositionOffset, offset, count);
		return result;
	}
}
