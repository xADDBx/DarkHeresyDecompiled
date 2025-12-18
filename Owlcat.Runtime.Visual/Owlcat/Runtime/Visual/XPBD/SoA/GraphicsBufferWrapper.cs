using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.SoA;

public abstract class GraphicsBufferWrapper
{
	protected GraphicsBuffer m_Buffer;

	private int m_NameId;

	private int m_Size;

	protected GraphicsBuffer.Target m_Target;

	public readonly string Name;

	public int NameId => m_NameId;

	public GraphicsBuffer Buffer => m_Buffer;

	public GraphicsBufferWrapper(string name, int size, GraphicsBuffer.Target target = GraphicsBuffer.Target.Structured)
	{
		Name = name;
		m_NameId = Shader.PropertyToID(name);
		m_Target = target;
		Resize(size);
	}

	public void Resize(int size)
	{
		m_Size = size;
		ResizeBuffer(size);
	}

	protected abstract void ResizeBuffer(int size);

	public void Dispose()
	{
		m_Buffer?.Dispose();
		m_Buffer = null;
	}

	public int GetSizeInBytes()
	{
		if (m_Buffer != null && m_Buffer.IsValid())
		{
			return m_Buffer.stride * m_Buffer.count;
		}
		return 0;
	}

	public void SetGlobal(CommandBuffer cmd)
	{
		cmd.SetGlobalBuffer(m_NameId, m_Buffer);
	}

	public static implicit operator GraphicsBuffer(GraphicsBufferWrapper wrapper)
	{
		return wrapper.m_Buffer;
	}
}
public class GraphicsBufferWrapper<T> : GraphicsBufferWrapper where T : struct
{
	public GraphicsBufferWrapper(string name, int size, GraphicsBuffer.Target target = GraphicsBuffer.Target.Structured)
		: base(name, size, target)
	{
	}

	public void SetData(NativeArray<T> data)
	{
		if (m_Buffer != null && m_Buffer.count > 0 && data.Length > 0)
		{
			m_Buffer.SetData(data);
		}
	}

	public void SetData(NativeArray<T> data, int managedBufferStartIndex, int graphicsBufferStartIndex, int count)
	{
		m_Buffer.SetData(data, managedBufferStartIndex, graphicsBufferStartIndex, count);
	}

	protected override void ResizeBuffer(int size)
	{
		if (m_Buffer == null || !m_Buffer.IsValid() || m_Buffer.count != size)
		{
			m_Buffer?.Dispose();
			m_Buffer = null;
			if (size > 0)
			{
				m_Buffer = new GraphicsBuffer(m_Target, size, Marshal.SizeOf<T>());
				m_Buffer.name = Name;
			}
		}
	}

	public static implicit operator GraphicsBuffer(GraphicsBufferWrapper<T> wrapper)
	{
		return wrapper.m_Buffer;
	}
}
