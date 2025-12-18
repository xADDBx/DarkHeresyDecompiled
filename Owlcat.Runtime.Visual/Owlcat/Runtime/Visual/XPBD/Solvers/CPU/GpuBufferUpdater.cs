using System;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Visual.XPBD.SoA;
using Unity.Collections;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Solvers.CPU;

public abstract class GpuBufferUpdater
{
	public abstract void Update(CommandBuffer cmd);
}
public class GpuBufferUpdater<T> : GpuBufferUpdater where T : struct
{
	private const int kMaxMemoryAmountPerFrame = 1048576;

	private int m_Length;

	private int m_Stride;

	private int m_MaxCount;

	private int m_Offset;

	private GraphicsBufferWrapper<T> m_Buffer;

	private Func<NativeArray<T>> m_DataGetter;

	public GpuBufferUpdater(Func<NativeArray<T>> dataGetter, GraphicsBufferWrapper<T> buffer)
	{
		m_DataGetter = dataGetter;
		m_Buffer = buffer;
		m_Length = 0;
		m_Stride = Marshal.SizeOf<T>();
		m_MaxCount = 1048576 / m_Stride;
	}

	public override void Update(CommandBuffer cmd)
	{
		int num = m_MaxCount;
		NativeArray<T> data = m_DataGetter();
		if (data.Length != m_Length)
		{
			m_Offset = 0;
			num = data.Length;
			m_Length = data.Length;
		}
		if (m_Offset + num > data.Length)
		{
			num = data.Length - m_Offset;
		}
		cmd.SetBufferData(m_Buffer.Buffer, data, m_Offset, m_Offset, num);
		m_Offset += num;
		if (m_Offset >= data.Length)
		{
			m_Offset = 0;
		}
	}
}
