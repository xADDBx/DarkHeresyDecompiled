using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class BodyVertexSoA : StructureOfArrays<BodyVertex>
{
	public NativeArray<float3> Normal;

	public NativeArray<float3> Position;

	public NativeArray<float3> RestNormal;

	public override BodyVertex this[int index]
	{
		get
		{
			BodyVertex result = default(BodyVertex);
			result.Normal = Normal[index];
			result.Position = Position[index];
			result.RestNormal = RestNormal[index];
			return result;
		}
		set
		{
			Normal[index] = value.Normal;
			Position[index] = value.Position;
			RestNormal[index] = value.RestNormal;
		}
	}

	public BodyVertexSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float3>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Normal = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Position = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		RestNormal = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		BodyVertexSoA bodyVertexSoA = (BodyVertexSoA)dst;
		NativeArray<float3>.Copy(Normal, offset, bodyVertexSoA.Normal, dstOffset, length);
		NativeArray<float3>.Copy(Position, offset, bodyVertexSoA.Position, dstOffset, length);
		NativeArray<float3>.Copy(RestNormal, offset, bodyVertexSoA.RestNormal, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		Normal.Dispose();
		Position.Dispose();
		RestNormal.Dispose();
	}

	public BodyVertexSoASlice GetSlice(int offset, int count)
	{
		BodyVertexSoASlice result = default(BodyVertexSoASlice);
		result.Normal = new NativeSlice<float3>(Normal, offset, count);
		result.Position = new NativeSlice<float3>(Position, offset, count);
		result.RestNormal = new NativeSlice<float3>(RestNormal, offset, count);
		return result;
	}
}
