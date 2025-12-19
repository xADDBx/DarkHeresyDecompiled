using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

public class GpuParticleSoA : GpuStructureOfArrays<Particle, ParticleSoA>
{
	public GraphicsBufferWrapper<float3> Velocity;

	public GraphicsBufferWrapper<float3> Position;

	public GraphicsBufferWrapper<int> JacobiPosCount;

	public GraphicsBufferWrapper<float> Radius;

	public GraphicsBufferWrapper<float3> BasePosition;

	public GraphicsBufferWrapper<float3> PrevPosition;

	public GraphicsBufferWrapper<float> InvMass;

	public GraphicsBufferWrapper<int3> JacobiPosDelta;

	public GpuParticleSoA(int size)
		: base(size)
	{
		Velocity = new GraphicsBufferWrapper<float3>("_XpbdParticleVelocityBuffer", size);
		m_Buffers.Add(Velocity);
		Position = new GraphicsBufferWrapper<float3>("_XpbdParticlePositionBuffer", size);
		m_Buffers.Add(Position);
		JacobiPosCount = new GraphicsBufferWrapper<int>("_XpbdParticleJacobiPosCountBuffer", size);
		m_Buffers.Add(JacobiPosCount);
		Radius = new GraphicsBufferWrapper<float>("_XpbdParticleRadiusBuffer", size);
		m_Buffers.Add(Radius);
		BasePosition = new GraphicsBufferWrapper<float3>("_XpbdParticleBasePositionBuffer", size);
		m_Buffers.Add(BasePosition);
		PrevPosition = new GraphicsBufferWrapper<float3>("_XpbdParticlePrevPositionBuffer", size);
		m_Buffers.Add(PrevPosition);
		InvMass = new GraphicsBufferWrapper<float>("_XpbdParticleInvMassBuffer", size);
		m_Buffers.Add(InvMass);
		JacobiPosDelta = new GraphicsBufferWrapper<int3>("_XpbdParticleJacobiPosDeltaBuffer", size);
		m_Buffers.Add(JacobiPosDelta);
	}

	public override void SetData(ParticleSoA data)
	{
		Velocity.SetData(data.Velocity);
		Position.SetData(data.Position);
		JacobiPosCount.SetData(data.JacobiPosCount);
		Radius.SetData(data.Radius);
		BasePosition.SetData(data.BasePosition);
		PrevPosition.SetData(data.PrevPosition);
		InvMass.SetData(data.InvMass);
		JacobiPosDelta.SetData(data.JacobiPosDelta);
	}

	public override void SetData(ParticleSoA data, int offset, int count)
	{
		Velocity.SetData(data.Velocity, offset, offset, count);
		Position.SetData(data.Position, offset, offset, count);
		JacobiPosCount.SetData(data.JacobiPosCount, offset, offset, count);
		Radius.SetData(data.Radius, offset, offset, count);
		BasePosition.SetData(data.BasePosition, offset, offset, count);
		PrevPosition.SetData(data.PrevPosition, offset, offset, count);
		InvMass.SetData(data.InvMass, offset, offset, count);
		JacobiPosDelta.SetData(data.JacobiPosDelta, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ParticleSoA data)
	{
		cmd.SetBufferData(Velocity.Buffer, data.Velocity);
		cmd.SetBufferData(Position.Buffer, data.Position);
		cmd.SetBufferData(JacobiPosCount.Buffer, data.JacobiPosCount);
		cmd.SetBufferData(Radius.Buffer, data.Radius);
		cmd.SetBufferData(BasePosition.Buffer, data.BasePosition);
		cmd.SetBufferData(PrevPosition.Buffer, data.PrevPosition);
		cmd.SetBufferData(InvMass.Buffer, data.InvMass);
		cmd.SetBufferData(JacobiPosDelta.Buffer, data.JacobiPosDelta);
	}

	public override void SetData(CommandBuffer cmd, ParticleSoA data, int offset, int count)
	{
		cmd.SetBufferData(Velocity.Buffer, data.Velocity, offset, offset, count);
		cmd.SetBufferData(Position.Buffer, data.Position, offset, offset, count);
		cmd.SetBufferData(JacobiPosCount.Buffer, data.JacobiPosCount, offset, offset, count);
		cmd.SetBufferData(Radius.Buffer, data.Radius, offset, offset, count);
		cmd.SetBufferData(BasePosition.Buffer, data.BasePosition, offset, offset, count);
		cmd.SetBufferData(PrevPosition.Buffer, data.PrevPosition, offset, offset, count);
		cmd.SetBufferData(InvMass.Buffer, data.InvMass, offset, offset, count);
		cmd.SetBufferData(JacobiPosDelta.Buffer, data.JacobiPosDelta, offset, offset, count);
	}
}
