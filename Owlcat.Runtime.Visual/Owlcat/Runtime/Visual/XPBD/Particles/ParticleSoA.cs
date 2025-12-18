using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

public class ParticleSoA : StructureOfArrays<Particle>
{
	public NativeArray<int> JacobiPosCount;

	public NativeArray<float3> PrevPosition;

	public NativeArray<float3> Velocity;

	public NativeArray<float3> Position;

	public NativeArray<float> InvMass;

	public NativeArray<float> Radius;

	public NativeArray<float3> BasePosition;

	public NativeArray<int3> JacobiPosDelta;

	public override Particle this[int index]
	{
		get
		{
			Particle result = default(Particle);
			result.JacobiPosCount = JacobiPosCount[index];
			result.PrevPosition = PrevPosition[index];
			result.Velocity = Velocity[index];
			result.Position = Position[index];
			result.InvMass = InvMass[index];
			result.Radius = Radius[index];
			result.BasePosition = BasePosition[index];
			result.JacobiPosDelta = JacobiPosDelta[index];
			return result;
		}
		set
		{
			JacobiPosCount[index] = value.JacobiPosCount;
			PrevPosition[index] = value.PrevPosition;
			Velocity[index] = value.Velocity;
			Position[index] = value.Position;
			InvMass[index] = value.InvMass;
			Radius[index] = value.Radius;
			BasePosition[index] = value.BasePosition;
			JacobiPosDelta[index] = value.JacobiPosDelta;
		}
	}

	public ParticleSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float>();
		num += Marshal.SizeOf<float>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<int3>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		JacobiPosCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevPosition = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Velocity = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Position = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		InvMass = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Radius = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BasePosition = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		JacobiPosDelta = new NativeArray<int3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ParticleSoA particleSoA = (ParticleSoA)dst;
		NativeArray<int>.Copy(JacobiPosCount, offset, particleSoA.JacobiPosCount, dstOffset, length);
		NativeArray<float3>.Copy(PrevPosition, offset, particleSoA.PrevPosition, dstOffset, length);
		NativeArray<float3>.Copy(Velocity, offset, particleSoA.Velocity, dstOffset, length);
		NativeArray<float3>.Copy(Position, offset, particleSoA.Position, dstOffset, length);
		NativeArray<float>.Copy(InvMass, offset, particleSoA.InvMass, dstOffset, length);
		NativeArray<float>.Copy(Radius, offset, particleSoA.Radius, dstOffset, length);
		NativeArray<float3>.Copy(BasePosition, offset, particleSoA.BasePosition, dstOffset, length);
		NativeArray<int3>.Copy(JacobiPosDelta, offset, particleSoA.JacobiPosDelta, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		JacobiPosCount.Dispose();
		PrevPosition.Dispose();
		Velocity.Dispose();
		Position.Dispose();
		InvMass.Dispose();
		Radius.Dispose();
		BasePosition.Dispose();
		JacobiPosDelta.Dispose();
	}

	public ParticleSoASlice GetSlice(int offset, int count)
	{
		ParticleSoASlice result = default(ParticleSoASlice);
		result.JacobiPosCount = new NativeSlice<int>(JacobiPosCount, offset, count);
		result.PrevPosition = new NativeSlice<float3>(PrevPosition, offset, count);
		result.Velocity = new NativeSlice<float3>(Velocity, offset, count);
		result.Position = new NativeSlice<float3>(Position, offset, count);
		result.InvMass = new NativeSlice<float>(InvMass, offset, count);
		result.Radius = new NativeSlice<float>(Radius, offset, count);
		result.BasePosition = new NativeSlice<float3>(BasePosition, offset, count);
		result.JacobiPosDelta = new NativeSlice<int3>(JacobiPosDelta, offset, count);
		return result;
	}
}
