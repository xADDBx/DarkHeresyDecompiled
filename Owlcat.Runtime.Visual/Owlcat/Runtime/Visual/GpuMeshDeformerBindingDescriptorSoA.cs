using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual;

public class GpuMeshDeformerBindingDescriptorSoA : GpuStructureOfArrays<MeshDeformerBindingDescriptor, MeshDeformerBindingDescriptorSoA>
{
	public GraphicsBufferWrapper<int> MasterIndexInSimulation;

	public GraphicsBufferWrapper<int4> Offsets;

	public GraphicsBufferWrapper<float4x4> BodyToDeformer;

	public GpuMeshDeformerBindingDescriptorSoA(int size)
		: base(size)
	{
		MasterIndexInSimulation = new GraphicsBufferWrapper<int>("_XpbdMeshDeformerBindingDescriptorMasterIndexInSimulationBuffer", size);
		m_Buffers.Add(MasterIndexInSimulation);
		Offsets = new GraphicsBufferWrapper<int4>("_XpbdMeshDeformerBindingDescriptorOffsetsBuffer", size);
		m_Buffers.Add(Offsets);
		BodyToDeformer = new GraphicsBufferWrapper<float4x4>("_XpbdMeshDeformerBindingDescriptorBodyToDeformerBuffer", size);
		m_Buffers.Add(BodyToDeformer);
	}

	public override void SetData(MeshDeformerBindingDescriptorSoA data)
	{
		MasterIndexInSimulation.SetData(data.MasterIndexInSimulation);
		Offsets.SetData(data.Offsets);
		BodyToDeformer.SetData(data.BodyToDeformer);
	}

	public override void SetData(MeshDeformerBindingDescriptorSoA data, int offset, int count)
	{
		MasterIndexInSimulation.SetData(data.MasterIndexInSimulation, offset, offset, count);
		Offsets.SetData(data.Offsets, offset, offset, count);
		BodyToDeformer.SetData(data.BodyToDeformer, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, MeshDeformerBindingDescriptorSoA data)
	{
		cmd.SetBufferData(MasterIndexInSimulation.Buffer, data.MasterIndexInSimulation);
		cmd.SetBufferData(Offsets.Buffer, data.Offsets);
		cmd.SetBufferData(BodyToDeformer.Buffer, data.BodyToDeformer);
	}

	public override void SetData(CommandBuffer cmd, MeshDeformerBindingDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(MasterIndexInSimulation.Buffer, data.MasterIndexInSimulation, offset, offset, count);
		cmd.SetBufferData(Offsets.Buffer, data.Offsets, offset, offset, count);
		cmd.SetBufferData(BodyToDeformer.Buffer, data.BodyToDeformer, offset, offset, count);
	}
}
