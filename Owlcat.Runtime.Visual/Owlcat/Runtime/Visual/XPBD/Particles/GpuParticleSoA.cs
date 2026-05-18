using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Particles;

public class GpuParticleSoA : GpuStructureOfArrays<Particle, ParticleSoA>
{
	public GraphicsBufferWrapper<float3> PrevPosition;

	public GraphicsBufferWrapper<int> JacobiPosCount;

	public GraphicsBufferWrapper<float> Radius;

	public GraphicsBufferWrapper<int3> JacobiPosDelta;

	public GraphicsBufferWrapper<float3> BasePosition;

	public GraphicsBufferWrapper<float3> Velocity;

	public GraphicsBufferWrapper<float> InvMass;

	public GraphicsBufferWrapper<float3> Position;

	public GpuParticleSoA(int size)
		: base(size)
	{
		PrevPosition = new GraphicsBufferWrapper<float3>("_XpbdParticlePrevPositionBuffer", size);
		m_Buffers.Add(PrevPosition);
		JacobiPosCount = new GraphicsBufferWrapper<int>("_XpbdParticleJacobiPosCountBuffer", size);
		m_Buffers.Add(JacobiPosCount);
		Radius = new GraphicsBufferWrapper<float>("_XpbdParticleRadiusBuffer", size);
		m_Buffers.Add(Radius);
		JacobiPosDelta = new GraphicsBufferWrapper<int3>("_XpbdParticleJacobiPosDeltaBuffer", size);
		m_Buffers.Add(JacobiPosDelta);
		BasePosition = new GraphicsBufferWrapper<float3>("_XpbdParticleBasePositionBuffer", size);
		m_Buffers.Add(BasePosition);
		Velocity = new GraphicsBufferWrapper<float3>("_XpbdParticleVelocityBuffer", size);
		m_Buffers.Add(Velocity);
		InvMass = new GraphicsBufferWrapper<float>("_XpbdParticleInvMassBuffer", size);
		m_Buffers.Add(InvMass);
		Position = new GraphicsBufferWrapper<float3>("_XpbdParticlePositionBuffer", size);
		m_Buffers.Add(Position);
	}

	public override void SetData(ParticleSoA data)
	{
		PrevPosition.SetData(data.PrevPosition);
		JacobiPosCount.SetData(data.JacobiPosCount);
		Radius.SetData(data.Radius);
		JacobiPosDelta.SetData(data.JacobiPosDelta);
		BasePosition.SetData(data.BasePosition);
		Velocity.SetData(data.Velocity);
		InvMass.SetData(data.InvMass);
		Position.SetData(data.Position);
	}

	public override void SetData(ParticleSoA data, int offset, int count)
	{
		PrevPosition.SetData(data.PrevPosition, offset, offset, count);
		JacobiPosCount.SetData(data.JacobiPosCount, offset, offset, count);
		Radius.SetData(data.Radius, offset, offset, count);
		JacobiPosDelta.SetData(data.JacobiPosDelta, offset, offset, count);
		BasePosition.SetData(data.BasePosition, offset, offset, count);
		Velocity.SetData(data.Velocity, offset, offset, count);
		InvMass.SetData(data.InvMass, offset, offset, count);
		Position.SetData(data.Position, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ParticleSoA data)
	{
		cmd.SetBufferData(PrevPosition.Buffer, data.PrevPosition);
		cmd.SetBufferData(JacobiPosCount.Buffer, data.JacobiPosCount);
		cmd.SetBufferData(Radius.Buffer, data.Radius);
		cmd.SetBufferData(JacobiPosDelta.Buffer, data.JacobiPosDelta);
		cmd.SetBufferData(BasePosition.Buffer, data.BasePosition);
		cmd.SetBufferData(Velocity.Buffer, data.Velocity);
		cmd.SetBufferData(InvMass.Buffer, data.InvMass);
		cmd.SetBufferData(Position.Buffer, data.Position);
	}

	public override void SetData(CommandBuffer cmd, ParticleSoA data, int offset, int count)
	{
		cmd.SetBufferData(PrevPosition.Buffer, data.PrevPosition, offset, offset, count);
		cmd.SetBufferData(JacobiPosCount.Buffer, data.JacobiPosCount, offset, offset, count);
		cmd.SetBufferData(Radius.Buffer, data.Radius, offset, offset, count);
		cmd.SetBufferData(JacobiPosDelta.Buffer, data.JacobiPosDelta, offset, offset, count);
		cmd.SetBufferData(BasePosition.Buffer, data.BasePosition, offset, offset, count);
		cmd.SetBufferData(Velocity.Buffer, data.Velocity, offset, offset, count);
		cmd.SetBufferData(InvMass.Buffer, data.InvMass, offset, offset, count);
		cmd.SetBufferData(Position.Buffer, data.Position, offset, offset, count);
	}
}
