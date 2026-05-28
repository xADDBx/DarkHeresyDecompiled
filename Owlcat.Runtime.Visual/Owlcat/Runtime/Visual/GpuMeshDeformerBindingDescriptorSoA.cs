using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual;

public class GpuMeshDeformerBindingDescriptorSoA : GpuStructureOfArrays<MeshDeformerBindingDescriptor, MeshDeformerBindingDescriptorSoA>
{
	public GraphicsBufferWrapper<int4> Offsets;

	public GraphicsBufferWrapper<float4x4> BodyToDeformer;

	public GraphicsBufferWrapper<int> MasterIndexInSimulation;

	public GpuMeshDeformerBindingDescriptorSoA(int size)
		: base(size)
	{
		Offsets = new GraphicsBufferWrapper<int4>("_XpbdMeshDeformerBindingDescriptorOffsetsBuffer", size);
		m_Buffers.Add(Offsets);
		BodyToDeformer = new GraphicsBufferWrapper<float4x4>("_XpbdMeshDeformerBindingDescriptorBodyToDeformerBuffer", size);
		m_Buffers.Add(BodyToDeformer);
		MasterIndexInSimulation = new GraphicsBufferWrapper<int>("_XpbdMeshDeformerBindingDescriptorMasterIndexInSimulationBuffer", size);
		m_Buffers.Add(MasterIndexInSimulation);
	}

	public override void SetData(MeshDeformerBindingDescriptorSoA data)
	{
		Offsets.SetData(data.Offsets);
		BodyToDeformer.SetData(data.BodyToDeformer);
		MasterIndexInSimulation.SetData(data.MasterIndexInSimulation);
	}

	public override void SetData(MeshDeformerBindingDescriptorSoA data, int offset, int count)
	{
		Offsets.SetData(data.Offsets, offset, offset, count);
		BodyToDeformer.SetData(data.BodyToDeformer, offset, offset, count);
		MasterIndexInSimulation.SetData(data.MasterIndexInSimulation, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, MeshDeformerBindingDescriptorSoA data)
	{
		cmd.SetBufferData(Offsets.Buffer, data.Offsets);
		cmd.SetBufferData(BodyToDeformer.Buffer, data.BodyToDeformer);
		cmd.SetBufferData(MasterIndexInSimulation.Buffer, data.MasterIndexInSimulation);
	}

	public override void SetData(CommandBuffer cmd, MeshDeformerBindingDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(Offsets.Buffer, data.Offsets, offset, offset, count);
		cmd.SetBufferData(BodyToDeformer.Buffer, data.BodyToDeformer, offset, offset, count);
		cmd.SetBufferData(MasterIndexInSimulation.Buffer, data.MasterIndexInSimulation, offset, offset, count);
	}
}
