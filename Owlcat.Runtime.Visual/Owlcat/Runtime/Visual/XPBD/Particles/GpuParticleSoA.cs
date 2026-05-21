using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

public class GpuParticleSoA : GpuStructureOfArrays<Particle, ParticleSoA>
{
	public GraphicsBufferWrapper<float3> Velocity;

	public GraphicsBufferWrapper<float> InvMass;

	public GraphicsBufferWrapper<float> Radius;

	public GraphicsBufferWrapper<float3> PrevPosition;

	public GraphicsBufferWrapper<float3> BasePosition;

	public GraphicsBufferWrapper<int3> JacobiPosDelta;

	public GraphicsBufferWrapper<float3> Position;

	public GraphicsBufferWrapper<int> JacobiPosCount;

	public GpuParticleSoA(int size)
		: base(size)
	{
		Velocity = new GraphicsBufferWrapper<float3>("_XpbdParticleVelocityBuffer", size);
		m_Buffers.Add(Velocity);
		InvMass = new GraphicsBufferWrapper<float>("_XpbdParticleInvMassBuffer", size);
		m_Buffers.Add(InvMass);
		Radius = new GraphicsBufferWrapper<float>("_XpbdParticleRadiusBuffer", size);
		m_Buffers.Add(Radius);
		PrevPosition = new GraphicsBufferWrapper<float3>("_XpbdParticlePrevPositionBuffer", size);
		m_Buffers.Add(PrevPosition);
		BasePosition = new GraphicsBufferWrapper<float3>("_XpbdParticleBasePositionBuffer", size);
		m_Buffers.Add(BasePosition);
		JacobiPosDelta = new GraphicsBufferWrapper<int3>("_XpbdParticleJacobiPosDeltaBuffer", size);
		m_Buffers.Add(JacobiPosDelta);
		Position = new GraphicsBufferWrapper<float3>("_XpbdParticlePositionBuffer", size);
		m_Buffers.Add(Position);
		JacobiPosCount = new GraphicsBufferWrapper<int>("_XpbdParticleJacobiPosCountBuffer", size);
		m_Buffers.Add(JacobiPosCount);
	}

	public override void SetData(ParticleSoA data)
	{
		Velocity.SetData(data.Velocity);
		InvMass.SetData(data.InvMass);
		Radius.SetData(data.Radius);
		PrevPosition.SetData(data.PrevPosition);
		BasePosition.SetData(data.BasePosition);
		JacobiPosDelta.SetData(data.JacobiPosDelta);
		Position.SetData(data.Position);
		JacobiPosCount.SetData(data.JacobiPosCount);
	}

	public override void SetData(ParticleSoA data, int offset, int count)
	{
		Velocity.SetData(data.Velocity, offset, offset, count);
		InvMass.SetData(data.InvMass, offset, offset, count);
		Radius.SetData(data.Radius, offset, offset, count);
		PrevPosition.SetData(data.PrevPosition, offset, offset, count);
		BasePosition.SetData(data.BasePosition, offset, offset, count);
		JacobiPosDelta.SetData(data.JacobiPosDelta, offset, offset, count);
		Position.SetData(data.Position, offset, offset, count);
		JacobiPosCount.SetData(data.JacobiPosCount, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ParticleSoA data)
	{
		cmd.SetBufferData(Velocity.Buffer, data.Velocity);
		cmd.SetBufferData(InvMass.Buffer, data.InvMass);
		cmd.SetBufferData(Radius.Buffer, data.Radius);
		cmd.SetBufferData(PrevPosition.Buffer, data.PrevPosition);
		cmd.SetBufferData(BasePosition.Buffer, data.BasePosition);
		cmd.SetBufferData(JacobiPosDelta.Buffer, data.JacobiPosDelta);
		cmd.SetBufferData(Position.Buffer, data.Position);
		cmd.SetBufferData(JacobiPosCount.Buffer, data.JacobiPosCount);
	}

	public override void SetData(CommandBuffer cmd, ParticleSoA data, int offset, int count)
	{
		cmd.SetBufferData(Velocity.Buffer, data.Velocity, offset, offset, count);
		cmd.SetBufferData(InvMass.Buffer, data.InvMass, offset, offset, count);
		cmd.SetBufferData(Radius.Buffer, data.Radius, offset, offset, count);
		cmd.SetBufferData(PrevPosition.Buffer, data.PrevPosition, offset, offset, count);
		cmd.SetBufferData(BasePosition.Buffer, data.BasePosition, offset, offset, count);
		cmd.SetBufferData(JacobiPosDelta.Buffer, data.JacobiPosDelta, offset, offset, count);
		cmd.SetBufferData(Position.Buffer, data.Position, offset, offset, count);
		cmd.SetBufferData(JacobiPosCount.Buffer, data.JacobiPosCount, offset, offset, count);
	}
}
