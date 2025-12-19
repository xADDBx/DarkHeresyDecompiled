using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Bodies;

public class GpuBodyBoneSoA : GpuStructureOfArrays<BodyBone, BodyBoneSoA>
{
	public GraphicsBufferWrapper<int> ParentIndex;

	public GraphicsBufferWrapper<float4x4> SimulatedBindpose;

	public GraphicsBufferWrapper<int> ParticleIndex;

	public GraphicsBufferWrapper<float4x4> Bonepose;

	public GraphicsBufferWrapper<float4x4> Bindpose;

	public GpuBodyBoneSoA(int size)
		: base(size)
	{
		ParentIndex = new GraphicsBufferWrapper<int>("_XpbdBodyBoneParentIndexBuffer", size);
		m_Buffers.Add(ParentIndex);
		SimulatedBindpose = new GraphicsBufferWrapper<float4x4>("_XpbdBodyBoneSimulatedBindposeBuffer", size);
		m_Buffers.Add(SimulatedBindpose);
		ParticleIndex = new GraphicsBufferWrapper<int>("_XpbdBodyBoneParticleIndexBuffer", size);
		m_Buffers.Add(ParticleIndex);
		Bonepose = new GraphicsBufferWrapper<float4x4>("_XpbdBodyBoneBoneposeBuffer", size);
		m_Buffers.Add(Bonepose);
		Bindpose = new GraphicsBufferWrapper<float4x4>("_XpbdBodyBoneBindposeBuffer", size);
		m_Buffers.Add(Bindpose);
	}

	public override void SetData(BodyBoneSoA data)
	{
		ParentIndex.SetData(data.ParentIndex);
		SimulatedBindpose.SetData(data.SimulatedBindpose);
		ParticleIndex.SetData(data.ParticleIndex);
		Bonepose.SetData(data.Bonepose);
		Bindpose.SetData(data.Bindpose);
	}

	public override void SetData(BodyBoneSoA data, int offset, int count)
	{
		ParentIndex.SetData(data.ParentIndex, offset, offset, count);
		SimulatedBindpose.SetData(data.SimulatedBindpose, offset, offset, count);
		ParticleIndex.SetData(data.ParticleIndex, offset, offset, count);
		Bonepose.SetData(data.Bonepose, offset, offset, count);
		Bindpose.SetData(data.Bindpose, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, BodyBoneSoA data)
	{
		cmd.SetBufferData(ParentIndex.Buffer, data.ParentIndex);
		cmd.SetBufferData(SimulatedBindpose.Buffer, data.SimulatedBindpose);
		cmd.SetBufferData(ParticleIndex.Buffer, data.ParticleIndex);
		cmd.SetBufferData(Bonepose.Buffer, data.Bonepose);
		cmd.SetBufferData(Bindpose.Buffer, data.Bindpose);
	}

	public override void SetData(CommandBuffer cmd, BodyBoneSoA data, int offset, int count)
	{
		cmd.SetBufferData(ParentIndex.Buffer, data.ParentIndex, offset, offset, count);
		cmd.SetBufferData(SimulatedBindpose.Buffer, data.SimulatedBindpose, offset, offset, count);
		cmd.SetBufferData(ParticleIndex.Buffer, data.ParticleIndex, offset, offset, count);
		cmd.SetBufferData(Bonepose.Buffer, data.Bonepose, offset, offset, count);
		cmd.SetBufferData(Bindpose.Buffer, data.Bindpose, offset, offset, count);
	}
}
