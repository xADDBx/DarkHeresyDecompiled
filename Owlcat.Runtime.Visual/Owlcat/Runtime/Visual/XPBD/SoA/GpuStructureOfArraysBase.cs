using System.Collections.Generic;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public abstract class GpuStructureOfArraysBase
{
	private int m_Capacity;

	protected List<GraphicsBufferWrapper> m_Buffers = new List<GraphicsBufferWrapper>();

	public int Capacity => m_Capacity;

	public GpuStructureOfArraysBase(int size)
	{
		m_Capacity = size;
	}

	public virtual void Dispose()
	{
		foreach (GraphicsBufferWrapper buffer in m_Buffers)
		{
			buffer.Dispose();
		}
		m_Buffers.Clear();
		m_Capacity = 0;
	}

	public void Resize(int newSize)
	{
		if (m_Capacity == newSize)
		{
			return;
		}
		foreach (GraphicsBufferWrapper buffer in m_Buffers)
		{
			buffer.Resize(newSize);
		}
		m_Capacity = newSize;
	}

	public void PushToGpu(CommandBuffer cmd)
	{
		foreach (GraphicsBufferWrapper buffer in m_Buffers)
		{
			cmd.SetGlobalBuffer(buffer.NameId, buffer.Buffer);
		}
	}

	public int GetSizeInBytes()
	{
		int num = 0;
		foreach (GraphicsBufferWrapper buffer in m_Buffers)
		{
			num += buffer.GetSizeInBytes();
		}
		return num;
	}
}
