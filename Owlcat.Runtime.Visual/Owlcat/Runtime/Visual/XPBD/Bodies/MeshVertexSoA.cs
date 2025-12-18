using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class MeshVertexSoA : StructureOfArrays<MeshVertex>
{
	public NativeArray<float3> Normal;

	public NativeArray<float3> Position;

	public override MeshVertex this[int index]
	{
		get
		{
			MeshVertex result = default(MeshVertex);
			result.Normal = Normal[index];
			result.Position = Position[index];
			return result;
		}
		set
		{
			Normal[index] = value.Normal;
			Position[index] = value.Position;
		}
	}

	public MeshVertexSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float3>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		Normal = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Position = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		MeshVertexSoA meshVertexSoA = (MeshVertexSoA)dst;
		NativeArray<float3>.Copy(Normal, offset, meshVertexSoA.Normal, dstOffset, length);
		NativeArray<float3>.Copy(Position, offset, meshVertexSoA.Position, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		Normal.Dispose();
		Position.Dispose();
	}

	public MeshVertexSoASlice GetSlice(int offset, int count)
	{
		MeshVertexSoASlice result = default(MeshVertexSoASlice);
		result.Normal = new NativeSlice<float3>(Normal, offset, count);
		result.Position = new NativeSlice<float3>(Position, offset, count);
		return result;
	}
}
