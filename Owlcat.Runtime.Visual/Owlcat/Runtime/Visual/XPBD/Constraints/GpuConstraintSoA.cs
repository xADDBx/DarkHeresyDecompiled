using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

public class GpuConstraintSoA : GpuStructureOfArrays<Constraint, ConstraintSoA>
{
	public GraphicsBufferWrapper<int4> Indices;

	public GraphicsBufferWrapper<float4> Parameters0;

	public GraphicsBufferWrapper<float4> Parameters1;

	public GpuConstraintSoA(int size)
		: base(size)
	{
		Indices = new GraphicsBufferWrapper<int4>("_XpbdConstraintIndicesBuffer", size);
		m_Buffers.Add(Indices);
		Parameters0 = new GraphicsBufferWrapper<float4>("_XpbdConstraintParameters0Buffer", size);
		m_Buffers.Add(Parameters0);
		Parameters1 = new GraphicsBufferWrapper<float4>("_XpbdConstraintParameters1Buffer", size);
		m_Buffers.Add(Parameters1);
	}

	public override void SetData(ConstraintSoA data)
	{
		Indices.SetData(data.Indices);
		Parameters0.SetData(data.Parameters0);
		Parameters1.SetData(data.Parameters1);
	}

	public override void SetData(ConstraintSoA data, int offset, int count)
	{
		Indices.SetData(data.Indices, offset, offset, count);
		Parameters0.SetData(data.Parameters0, offset, offset, count);
		Parameters1.SetData(data.Parameters1, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ConstraintSoA data)
	{
		cmd.SetBufferData(Indices.Buffer, data.Indices);
		cmd.SetBufferData(Parameters0.Buffer, data.Parameters0);
		cmd.SetBufferData(Parameters1.Buffer, data.Parameters1);
	}

	public override void SetData(CommandBuffer cmd, ConstraintSoA data, int offset, int count)
	{
		cmd.SetBufferData(Indices.Buffer, data.Indices, offset, offset, count);
		cmd.SetBufferData(Parameters0.Buffer, data.Parameters0, offset, offset, count);
		cmd.SetBufferData(Parameters1.Buffer, data.Parameters1, offset, offset, count);
	}
}
