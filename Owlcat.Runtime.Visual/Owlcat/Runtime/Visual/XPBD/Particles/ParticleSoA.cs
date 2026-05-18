using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using Unity.Mathematics;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

public class ParticleSoA : StructureOfArrays<Particle>
{
	public NativeArray<float3> PrevPosition;

	public NativeArray<int> JacobiPosCount;

	public NativeArray<float> Radius;

	public NativeArray<int3> JacobiPosDelta;

	public NativeArray<float3> BasePosition;

	public NativeArray<float3> Velocity;

	public NativeArray<float> InvMass;

	public NativeArray<float3> Position;

	public override Particle this[int index]
	{
		get
		{
			Particle result = default(Particle);
			result.PrevPosition = PrevPosition[index];
			result.JacobiPosCount = JacobiPosCount[index];
			result.Radius = Radius[index];
			result.JacobiPosDelta = JacobiPosDelta[index];
			result.BasePosition = BasePosition[index];
			result.Velocity = Velocity[index];
			result.InvMass = InvMass[index];
			result.Position = Position[index];
			return result;
		}
		set
		{
			PrevPosition[index] = value.PrevPosition;
			JacobiPosCount[index] = value.JacobiPosCount;
			Radius[index] = value.Radius;
			JacobiPosDelta[index] = value.JacobiPosDelta;
			BasePosition[index] = value.BasePosition;
			Velocity[index] = value.Velocity;
			InvMass[index] = value.InvMass;
			Position[index] = value.Position;
		}
	}

	public ParticleSoA(int size)
		: base(size)
	{
		int num = 0;
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<int>();
		num += Marshal.SizeOf<float>();
		num += Marshal.SizeOf<int3>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float3>();
		num += Marshal.SizeOf<float>();
		num += Marshal.SizeOf<float3>();
		m_Allocator.Stride = num;
	}

	public override void Resize(int newSize)
	{
		base.Resize(newSize);
		PrevPosition = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		JacobiPosCount = new NativeArray<int>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Radius = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		JacobiPosDelta = new NativeArray<int3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		BasePosition = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Velocity = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		InvMass = new NativeArray<float>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		Position = new NativeArray<float3>(newSize, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
	}

	public override void CopyTo(StructureOfArraysBase dst, int offset, int dstOffset, int length)
	{
		ParticleSoA particleSoA = (ParticleSoA)dst;
		NativeArray<float3>.Copy(PrevPosition, offset, particleSoA.PrevPosition, dstOffset, length);
		NativeArray<int>.Copy(JacobiPosCount, offset, particleSoA.JacobiPosCount, dstOffset, length);
		NativeArray<float>.Copy(Radius, offset, particleSoA.Radius, dstOffset, length);
		NativeArray<int3>.Copy(JacobiPosDelta, offset, particleSoA.JacobiPosDelta, dstOffset, length);
		NativeArray<float3>.Copy(BasePosition, offset, particleSoA.BasePosition, dstOffset, length);
		NativeArray<float3>.Copy(Velocity, offset, particleSoA.Velocity, dstOffset, length);
		NativeArray<float>.Copy(InvMass, offset, particleSoA.InvMass, dstOffset, length);
		NativeArray<float3>.Copy(Position, offset, particleSoA.Position, dstOffset, length);
	}

	public override void Dispose()
	{
		base.Dispose();
		PrevPosition.Dispose();
		JacobiPosCount.Dispose();
		Radius.Dispose();
		JacobiPosDelta.Dispose();
		BasePosition.Dispose();
		Velocity.Dispose();
		InvMass.Dispose();
		Position.Dispose();
	}

	public ParticleSoASlice GetSlice(int offset, int count)
	{
		ParticleSoASlice result = default(ParticleSoASlice);
		result.PrevPosition = new NativeSlice<float3>(PrevPosition, offset, count);
		result.JacobiPosCount = new NativeSlice<int>(JacobiPosCount, offset, count);
		result.Radius = new NativeSlice<float>(Radius, offset, count);
		result.JacobiPosDelta = new NativeSlice<int3>(JacobiPosDelta, offset, count);
		result.BasePosition = new NativeSlice<float3>(BasePosition, offset, count);
		result.Velocity = new NativeSlice<float3>(Velocity, offset, count);
		result.InvMass = new NativeSlice<float>(InvMass, offset, count);
		result.Position = new NativeSlice<float3>(Position, offset, count);
		return result;
	}
}
