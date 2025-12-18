using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Math;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public struct ResizableGraphicsBuffer : IDisposable
{
	private GraphicsBufferHandle m_Handle;

	private int m_Count;

	private int m_Stride;

	public GraphicsBuffer InternalBuffer { get; private set; }

	public bool IsCreated => InternalBuffer != null;

	public int Count
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Count;
		}
	}

	public GraphicsBufferHandle BufferHandle
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Handle;
		}
	}

	public int Stride
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get
		{
			return m_Stride;
		}
	}

	public string Name
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		set
		{
			InternalBuffer.name = value;
		}
	}

	public ResizableGraphicsBuffer(GraphicsBuffer.Target target, int count, int stride)
	{
		InternalBuffer = new GraphicsBuffer(target, count, stride);
		m_Handle = InternalBuffer.bufferHandle;
		m_Count = count;
		m_Stride = stride;
	}

	public void CreateOrResize(GraphicsBuffer.Target target, int count, int stride)
	{
		if (!IsCreated)
		{
			InternalBuffer = new GraphicsBuffer(target, count, stride);
			m_Handle = InternalBuffer.bufferHandle;
			m_Count = count;
			m_Stride = stride;
		}
		else
		{
			Resize(count);
		}
	}

	public void Resize(int ensuredCapacity)
	{
		if (ResizeImpl(ensuredCapacity, out var oldBuffer))
		{
			oldBuffer.Release();
		}
	}

	public void Resize(int ensuredCapacity, GPUDrivenCommandQueue commandQueue)
	{
		if (ResizeImpl(ensuredCapacity, out var oldBuffer))
		{
			commandQueue.DeferDelete(oldBuffer);
		}
	}

	public void ResizeKeepContentsAndClear(int ensuredCapacity, GPUDrivenBufferUtils bufferUtils, GPUDrivenCommandQueue commandQueue, int clearValue)
	{
		if (ResizeImpl(ensuredCapacity, out var oldBuffer))
		{
			int num = oldBuffer.count * oldBuffer.stride;
			bufferUtils.DispatchCopyBuffer(commandQueue.Cmd, oldBuffer, InternalBuffer, num);
			int sizeInBytes = InternalBuffer.count * InternalBuffer.stride - num;
			bufferUtils.DispatchClearBuffer(commandQueue.Cmd, InternalBuffer, clearValue, num, sizeInBytes);
			commandQueue.DeferDelete(oldBuffer);
		}
	}

	private bool ResizeImpl(int ensuredCapacity, out GraphicsBuffer oldBuffer)
	{
		int count = Count;
		if (ensuredCapacity <= count)
		{
			oldBuffer = null;
			return false;
		}
		int count2 = Alignment.AlignUp(ensuredCapacity, count);
		GraphicsBuffer.Target target = InternalBuffer.target;
		int stride = InternalBuffer.stride;
		GraphicsBuffer internalBuffer = new GraphicsBuffer(target, count2, stride);
		oldBuffer = InternalBuffer;
		InternalBuffer = internalBuffer;
		m_Handle = InternalBuffer.bufferHandle;
		m_Count = count2;
		return true;
	}

	[Conditional("BUFFER_VALIDATION")]
	private void EnsureCreated()
	{
	}

	[Conditional("BUFFER_VALIDATION")]
	private void EnsureValid()
	{
	}

	public void Dispose()
	{
		if (InternalBuffer != null)
		{
			InternalBuffer.Dispose();
			InternalBuffer = null;
			m_Handle = default(GraphicsBufferHandle);
			m_Count = 0;
			m_Stride = 0;
		}
	}
}
