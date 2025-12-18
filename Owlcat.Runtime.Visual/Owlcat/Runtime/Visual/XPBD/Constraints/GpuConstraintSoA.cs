using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Constraints;

public class GpuConstraintSoA : GpuStructureOfArrays<Constraint, ConstraintSoA>
{
	public GraphicsBufferWrapper<float4> Parameters1;

	public GraphicsBufferWrapper<float4> Parameters0;

	public GraphicsBufferWrapper<int4> Indices;

	public GpuConstraintSoA(int size)
		: base(size)
	{
		Parameters1 = new GraphicsBufferWrapper<float4>("_XpbdConstraintParameters1Buffer", size);
		m_Buffers.Add(Parameters1);
		Parameters0 = new GraphicsBufferWrapper<float4>("_XpbdConstraintParameters0Buffer", size);
		m_Buffers.Add(Parameters0);
		Indices = new GraphicsBufferWrapper<int4>("_XpbdConstraintIndicesBuffer", size);
		m_Buffers.Add(Indices);
	}

	public override void SetData(ConstraintSoA data)
	{
		Parameters1.SetData(data.Parameters1);
		Parameters0.SetData(data.Parameters0);
		Indices.SetData(data.Indices);
	}

	public override void SetData(ConstraintSoA data, int offset, int count)
	{
		Parameters1.SetData(data.Parameters1, offset, offset, count);
		Parameters0.SetData(data.Parameters0, offset, offset, count);
		Indices.SetData(data.Indices, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ConstraintSoA data)
	{
		cmd.SetBufferData(Parameters1.Buffer, data.Parameters1);
		cmd.SetBufferData(Parameters0.Buffer, data.Parameters0);
		cmd.SetBufferData(Indices.Buffer, data.Indices);
	}

	public override void SetData(CommandBuffer cmd, ConstraintSoA data, int offset, int count)
	{
		cmd.SetBufferData(Parameters1.Buffer, data.Parameters1, offset, offset, count);
		cmd.SetBufferData(Parameters0.Buffer, data.Parameters0, offset, offset, count);
		cmd.SetBufferData(Indices.Buffer, data.Indices, offset, offset, count);
	}
}
