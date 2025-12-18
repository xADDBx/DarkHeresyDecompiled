using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public class GpuSingleArraySoA<T> : GpuStructureOfArrays<T, SingleArraySoA<T>> where T : struct
{
	private GraphicsBufferWrapper<T> m_Buffer;

	public GraphicsBufferWrapper<T> Buffer => m_Buffer;

	public GpuSingleArraySoA(string name, int size)
		: base(size)
	{
		m_Buffer = new GraphicsBufferWrapper<T>(name, size);
		m_Buffers.Add(m_Buffer);
	}

	public override void SetData(SingleArraySoA<T> data)
	{
		m_Buffer.SetData(data.Array);
	}

	public override void SetData(SingleArraySoA<T> data, int offset, int count)
	{
		m_Buffer.SetData(data.Array, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, SingleArraySoA<T> data)
	{
		cmd.SetBufferData(m_Buffer, data.Array);
	}

	public override void SetData(CommandBuffer cmd, SingleArraySoA<T> data, int offset, int count)
	{
		cmd.SetBufferData(m_Buffer, data.Array, offset, offset, count);
	}
}
