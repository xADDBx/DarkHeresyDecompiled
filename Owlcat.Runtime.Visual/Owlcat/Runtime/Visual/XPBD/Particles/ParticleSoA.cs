using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

public class ParticleSoA : StructureOfArrays<Particle>
{
	public NativeArray<int3> JacobiPosDelta;

	public NativeArray<int> JacobiPosCount;

	public NativeArray<float3> Velocity;

	public NativeArray<float> InvMass;

	public NativeArray<float> Radius;

	public NativeArray<float3> Position;

	public NativeArray<float3> PrevPosition;

	public NativeArray<float3> BasePosition;

	public override Particle this[int index]
	{
		get
		{
			Particle result = default(Particle);
			result.JacobiPosDelta = JacobiPosDelta[index];
			result.JacobiPosCount = JacobiPosCount[index];
			result.Velocity = Velocity[index];
			result.InvMass = InvMass[index];
			result.Radius = Radius[index];
			result.Position = Position[index];
			result.PrevPosition = PrevPosition[index];
			result.BasePosition = BasePosition[index];
			return result;
		}
		set
		{
			JacobiPosDelta[index] = value.JacobiPosDelta;
			JacobiPosCount[index] = value.JacobiPosCount;
			Velocity[index] = value.Velocity;
			InvMass[index] = value.InvMass;
			Radius[index] = value.Radius;
			Position[index] = value.Position;
			PrevPosition[index] = value.PrevPosition;
			BasePosition[index] = value.BasePosition;
		}
	}

	public ParticleSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<int3>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float>();
		num += Marshal.SizeOf<float>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float3>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		JacobiPosDelta = new NativeArray<int3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		JacobiPosCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Velocity = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		InvMass = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Radius = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Position = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		PrevPosition = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BasePosition = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ParticleSoA particleSoA = (ParticleSoA)dst;
		NativeArray<int3>.Copy(JacobiPosDelta, offset, particleSoA.JacobiPosDelta, dstOffset, length);
		NativeArray<int>.Copy(JacobiPosCount, offset, particleSoA.JacobiPosCount, dstOffset, length);
		NativeArray<float3>.Copy(Velocity, offset, particleSoA.Velocity, dstOffset, length);
		NativeArray<float>.Copy(InvMass, offset, particleSoA.InvMass, dstOffset, length);
		NativeArray<float>.Copy(Radius, offset, particleSoA.Radius, dstOffset, length);
		NativeArray<float3>.Copy(Position, offset, particleSoA.Position, dstOffset, length);
		NativeArray<float3>.Copy(PrevPosition, offset, particleSoA.PrevPosition, dstOffset, length);
		NativeArray<float3>.Copy(BasePosition, offset, particleSoA.BasePosition, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		JacobiPosDelta.Dispose();
		JacobiPosCount.Dispose();
		Velocity.Dispose();
		InvMass.Dispose();
		Radius.Dispose();
		Position.Dispose();
		PrevPosition.Dispose();
		BasePosition.Dispose();
	}

	public ParticleSoASlice GetSlice(int offset, int count)
	{
		ParticleSoASlice result = default(ParticleSoASlice);
		result.JacobiPosDelta = new NativeSlice<int3>(JacobiPosDelta, offset, count);
		result.JacobiPosCount = new NativeSlice<int>(JacobiPosCount, offset, count);
		result.Velocity = new NativeSlice<float3>(Velocity, offset, count);
		result.InvMass = new NativeSlice<float>(InvMass, offset, count);
		result.Radius = new NativeSlice<float>(Radius, offset, count);
		result.Position = new NativeSlice<float3>(Position, offset, count);
		result.PrevPosition = new NativeSlice<float3>(PrevPosition, offset, count);
		result.BasePosition = new NativeSlice<float3>(BasePosition, offset, count);
		return result;
	}
}
