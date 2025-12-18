using System;
using System.Collections.Generic;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenBatchedDataUploader : IDisposable
{
	public struct StagingBuffer : IDisposable
	{
		public NativeArray<byte> CPUDataBuffer;

		public GraphicsBuffer GPUDataBuffer;

		public NativeList<GPUDrivenUploadSegment> CPUSegmentsBuffer;

		public GraphicsBuffer GPUSegmentsBuffer;

		public int CurrentOffset;

		public int LastUsedFrameIndex;

		public bool IsCreated => CPUDataBuffer.IsCreated;

		public void Reset()
		{
			CurrentOffset = 0;
			CPUSegmentsBuffer.Clear();
		}

		public void Dispose()
		{
			if (CPUDataBuffer.IsCreated)
			{
				CPUDataBuffer.Dispose();
				CPUDataBuffer = default(NativeArray<byte>);
			}
			GPUDataBuffer?.Dispose();
			GPUDataBuffer = null;
			if (CPUSegmentsBuffer.IsCreated)
			{
				CPUSegmentsBuffer.Dispose();
				CPUSegmentsBuffer = default(NativeList<GPUDrivenUploadSegment>);
			}
			GPUSegmentsBuffer?.Dispose();
			GPUSegmentsBuffer = null;
		}
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler Flush = new ProfilingSampler("Batch Data Upload: Flush");
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@01c4fcbc474f\\Runtime\\GPUDrivenBRG\\GPUDrivenBatchedDataUploader.cs", needAccessors = false)]
	public struct GPUDrivenUploadSegment
	{
		public uint SourceOffset;

		public uint DestinationOffset;

		public uint Count;

		public uint Padding;
	}

	private static class ShaderIDs
	{
		public static int _Source = Shader.PropertyToID("_Source");

		public static int _Destination = Shader.PropertyToID("_Destination");

		public static int _Segments = Shader.PropertyToID("_Segments");
	}

	private readonly ComputeShader m_DataUploadCS;

	private readonly Queue<StagingBuffer> m_FreeStagingBuffers = new Queue<StagingBuffer>();

	private readonly Queue<StagingBuffer> m_StagingBuffersInFlight = new Queue<StagingBuffer>();

	private int m_CurrentFrameIndex;

	private StagingBuffer m_CurrentStagingBuffer;

	public GPUUploadMode Mode { get; }

	public GPUDrivenBatchedDataUploader(PipelineRuntimeResources pipelineRuntimeResources, GPUUploadMode mode)
	{
		m_DataUploadCS = pipelineRuntimeResources.DataUploadCS;
		Mode = mode;
	}

	public void Dispose()
	{
		StagingBuffer result;
		while (m_FreeStagingBuffers.TryDequeue(out result))
		{
			result.Dispose();
		}
		StagingBuffer result2;
		while (m_StagingBuffersInFlight.TryDequeue(out result2))
		{
			result2.Dispose();
		}
	}

	public void PostRender()
	{
		m_CurrentFrameIndex++;
	}

	public void ResetCurrent()
	{
		if (m_CurrentStagingBuffer.IsCreated)
		{
			m_CurrentStagingBuffer.Reset();
			m_FreeStagingBuffers.Enqueue(m_CurrentStagingBuffer);
		}
	}

	public ref StagingBuffer GetOrAllocateCurrentStagingBuffer(int requiredByteCapacity, int requiredSegmentCapacity)
	{
		if (!m_CurrentStagingBuffer.IsCreated)
		{
			m_CurrentStagingBuffer = GetFreeStagingBuffer();
		}
		if (m_CurrentStagingBuffer.CPUDataBuffer.Length < requiredByteCapacity)
		{
			int capacity = Mathf.Max(m_CurrentStagingBuffer.CPUDataBuffer.Length * 2, requiredByteCapacity);
			ArrayExtensions.ResizeArray(ref m_CurrentStagingBuffer.CPUDataBuffer, capacity);
		}
		if (m_CurrentStagingBuffer.CPUSegmentsBuffer.Capacity < requiredSegmentCapacity)
		{
			m_CurrentStagingBuffer.CPUSegmentsBuffer.SetCapacity(requiredSegmentCapacity);
		}
		return ref m_CurrentStagingBuffer;
	}

	[BurstCompile]
	public unsafe static void CopyDataToStagingBuffer(NativeArray<byte> cpuDataBuffer, byte* source, int sourceOffset, int count, int currentOffset)
	{
		UnsafeUtility.MemCpy((byte*)cpuDataBuffer.GetUnsafePtr() + currentOffset, source + sourceOffset, count);
	}

	[BurstCompile]
	public static GPUDrivenUploadSegment CreateUploadSegment(int currentOffset, int destinationOffset, int count)
	{
		GPUDrivenUploadSegment result = default(GPUDrivenUploadSegment);
		result.SourceOffset = (uint)currentOffset;
		result.DestinationOffset = (uint)destinationOffset;
		result.Count = (uint)count;
		return result;
	}

	public void Flush(CommandBuffer cmd, GraphicsBuffer destination)
	{
		if (!m_CurrentStagingBuffer.IsCreated)
		{
			return;
		}
		using (new ProfilingScope(cmd, Profiling.Flush))
		{
			StagingBuffer currentStagingBuffer = m_CurrentStagingBuffer;
			if (currentStagingBuffer.CurrentOffset > 0)
			{
				NativeList<GPUDrivenUploadSegment> cPUSegmentsBuffer = currentStagingBuffer.CPUSegmentsBuffer;
				if (cPUSegmentsBuffer.Length > 0)
				{
					int length = m_CurrentStagingBuffer.CPUSegmentsBuffer.Length;
					EnsureBufferCapacity(ref m_CurrentStagingBuffer.GPUDataBuffer, m_CurrentStagingBuffer.CurrentOffset, GraphicsBuffer.Target.Raw, 4);
					EnsureBufferCapacity(ref m_CurrentStagingBuffer.GPUSegmentsBuffer, length * UnsafeUtility.SizeOf<GPUDrivenUploadSegment>(), GraphicsBuffer.Target.Structured, UnsafeUtility.SizeOf<GPUDrivenUploadSegment>());
					m_CurrentStagingBuffer.GPUDataBuffer.SetData(m_CurrentStagingBuffer.CPUDataBuffer, 0, 0, m_CurrentStagingBuffer.CurrentOffset);
					m_CurrentStagingBuffer.GPUSegmentsBuffer.SetData(m_CurrentStagingBuffer.CPUSegmentsBuffer.AsArray());
					cmd.SetComputeBufferParam(m_DataUploadCS, 0, ShaderIDs._Source, m_CurrentStagingBuffer.GPUDataBuffer);
					cmd.SetComputeBufferParam(m_DataUploadCS, 0, ShaderIDs._Destination, destination);
					cmd.SetComputeBufferParam(m_DataUploadCS, 0, ShaderIDs._Segments, m_CurrentStagingBuffer.GPUSegmentsBuffer);
					cmd.DispatchCompute(m_DataUploadCS, 0, length, 1, 1);
					m_CurrentStagingBuffer.Reset();
					m_CurrentStagingBuffer.LastUsedFrameIndex = m_CurrentFrameIndex;
					m_StagingBuffersInFlight.Enqueue(m_CurrentStagingBuffer);
					goto IL_017f;
				}
			}
			m_CurrentStagingBuffer.Reset();
			m_FreeStagingBuffers.Enqueue(m_CurrentStagingBuffer);
			goto IL_017f;
			IL_017f:
			m_CurrentStagingBuffer = default(StagingBuffer);
		}
	}

	private static void EnsureBufferCapacity(ref GraphicsBuffer graphicsBuffer, int ensuredCapacity, GraphicsBuffer.Target bufferTarget, int stride)
	{
		int num = ((graphicsBuffer != null) ? (graphicsBuffer.count * graphicsBuffer.stride) : 0);
		if (num < ensuredCapacity)
		{
			graphicsBuffer?.Dispose();
			graphicsBuffer = null;
			int value = Mathf.Max(num * 2, ensuredCapacity);
			graphicsBuffer = new GraphicsBuffer(bufferTarget, Alignment.AlignUp(value, stride) / stride, stride);
		}
	}

	private StagingBuffer GetFreeStagingBuffer()
	{
		if (m_FreeStagingBuffers.TryDequeue(out var result))
		{
			result.Reset();
			return result;
		}
		if (m_StagingBuffersInFlight.TryPeek(out result) && m_CurrentFrameIndex > result.LastUsedFrameIndex + 3)
		{
			m_StagingBuffersInFlight.Dequeue();
			result.Reset();
			return result;
		}
		StagingBuffer result2 = default(StagingBuffer);
		result2.CurrentOffset = 0;
		result2.CPUDataBuffer = new NativeArray<byte>(8192, Allocator.Persistent);
		result2.CPUSegmentsBuffer = new NativeList<GPUDrivenUploadSegment>(Allocator.Persistent);
		return result2;
	}
}
