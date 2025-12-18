using System;
using System.Runtime.CompilerServices;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenPersistentData : IDisposable, IGPUDrivenMemoryProfilingSource
{
	private static class ShaderIDs
	{
		public static int _PerInstanceMetadata = Shader.PropertyToID("_PerInstanceMetadata");
	}

	public struct PendingGPUUpload : IDisposable
	{
		public NativeSparseSegmentList.SegmentMergeJobInfo DirtyData;

		public NativeSparseSegmentList.SegmentMergeJobInfo DirtyVisibilityInfo;

		public NativeSparseSegmentList.SegmentMergeJobInfo DirtyPerInstanceMetadata;

		public NativeSparseSegmentList.SegmentMergeJobInfo LODGroupData;

		public void Dispose()
		{
			DirtyData.Dispose();
			DirtyVisibilityInfo.Dispose();
			DirtyPerInstanceMetadata.Dispose();
			LODGroupData.Dispose();
		}
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler UploadComplete = new ProfilingSampler("Upload: Complete");

		public static readonly ProfilingSampler UploadPersistentData = new ProfilingSampler("Upload: Persistent Material and Instance Data");

		public static readonly ProfilingSampler UploadVisibilityInfo = new ProfilingSampler("Upload: Visibility Info");

		public static readonly ProfilingSampler UploadPerInstanceMetadata = new ProfilingSampler("Upload: Per Instance Metadata");

		public static readonly ProfilingSampler UploadLODGroupData = new ProfilingSampler("Upload: LOD Group Data");

		public static readonly ProfilingSampler UploadDebug = new ProfilingSampler("Upload: Add Debug Segments");
	}

	private const int kMaterialDataSizeAlignment = 128;

	private const int kGPUPersistentBufferStride = 4;

	private static readonly int s_CPUInstanceDataStride = UnsafeUtility.SizeOf<float4>();

	private readonly GPUDrivenBatchRendererGroup m_BRG;

	private readonly GPUDrivenAllocator m_DataAllocator;

	private readonly GPUDrivenBatchedDataUploader m_DataUploader;

	private readonly GPUDrivenLODGroupRepository m_LODGroupRepository;

	private NativeArray<float4> m_CPUInstanceData;

	private NativeSparseSegmentList m_DirtyDataSegmentList;

	private NativeSparseSegmentList m_DirtyPerInstanceMetadataSegmentList;

	private NativeSparseSegmentList m_DirtyVisibilityInfoSegmentList;

	private ResizableGraphicsBuffer m_GPUPersistentInstanceData;

	private NativeArray<GPUDrivenAllocator.DataAllocation> m_InstanceAllocations;

	private NativeArray<GPUDrivenPerInstanceMetadata> m_PerInstanceMetadata;

	private ResizableGraphicsBuffer m_PerInstanceMetadataBuffer;

	private NativeArray<GPUDrivenVisibilityInfo> m_VisibilityInfo;

	private ResizableGraphicsBuffer m_VisibilityInfoBuffer;

	public ref readonly ResizableGraphicsBuffer VisibilityInfoBuffer => ref m_VisibilityInfoBuffer;

	public ref readonly ResizableGraphicsBuffer GPUPersistentInstanceData => ref m_GPUPersistentInstanceData;

	public int ReferenceInstanceSizeInBytes { get; }

	public GPUDrivenPersistentData(GPUDrivenBatchRendererGroup brg, GPUDrivenBatchedDataUploader dataUploader, GPUDrivenLODGroupRepository lodGroupRepository)
	{
		m_BRG = brg;
		m_DataUploader = dataUploader;
		ReferenceInstanceSizeInBytes = UnsafeUtility.SizeOf<GPUDrivenMetadataAuthoring.DefaultPerInstanceData>() + UnsafeUtility.SizeOf<float4>() * 8 + math.max(UnsafeUtility.SizeOf<GPUDrivenMetadataAuthoring.LightMapsPerInstanceData>(), UnsafeUtility.SizeOf<GPUDrivenMetadataAuthoring.LightProbesPerInstanceData>());
		m_DataAllocator = new GPUDrivenAllocator(m_BRG.Settings);
		m_LODGroupRepository = lodGroupRepository;
		m_DirtyDataSegmentList = new NativeSparseSegmentList(Allocator.Persistent, s_CPUInstanceDataStride);
		m_DirtyVisibilityInfoSegmentList = new NativeSparseSegmentList(Allocator.Persistent);
		m_DirtyPerInstanceMetadataSegmentList = new NativeSparseSegmentList(Allocator.Persistent);
		GrowOrCreateInstanceBuffers(m_BRG.Settings.InitialInstanceCapacity);
		int num = Alignment.AlignUp(m_BRG.Settings.InitialMaterialCapacity * m_BRG.Settings.ReferenceMaterialSizeInBytes, s_CPUInstanceDataStride);
		int num2 = Alignment.AlignUp(m_BRG.Settings.InitialInstanceCapacity * ReferenceInstanceSizeInBytes, s_CPUInstanceDataStride);
		int size = num + num2;
		GrowOrCreatePersistentDataBuffers(size);
	}

	public void Dispose()
	{
		DisposeCollectionIfCreated(ref m_InstanceAllocations);
		DisposeCollectionIfCreated(ref m_VisibilityInfo);
		m_DataAllocator?.Dispose();
		m_GPUPersistentInstanceData.Dispose();
		if (m_CPUInstanceData.IsCreated)
		{
			m_CPUInstanceData.Dispose();
			m_CPUInstanceData = default(NativeArray<float4>);
		}
		m_VisibilityInfoBuffer.Dispose();
		m_DirtyDataSegmentList.Dispose();
		m_DirtyVisibilityInfoSegmentList.Dispose();
		m_DirtyPerInstanceMetadataSegmentList.Dispose();
		if (m_PerInstanceMetadata.IsCreated)
		{
			m_PerInstanceMetadata.Dispose();
			m_PerInstanceMetadata = default(NativeArray<GPUDrivenPerInstanceMetadata>);
		}
		m_PerInstanceMetadataBuffer.Dispose();
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.InstanceDataCPU, m_CPUInstanceData);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_InstanceAllocations);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_PerInstanceMetadata);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_VisibilityInfo);
		counters.CollectBufferSize(counters.InstanceDataGPU, m_GPUPersistentInstanceData);
		counters.CollectBufferSize(counters.InstanceDataGPU, m_PerInstanceMetadataBuffer);
		counters.CollectBufferSize(counters.InstanceDataGPU, m_VisibilityInfoBuffer);
		ProfilerCounterValue<int> instanceDataCPU = counters.InstanceDataCPU;
		instanceDataCPU.Value += m_DirtyDataSegmentList.TotalMemoryUsed;
		instanceDataCPU = counters.InstanceDataCPU;
		instanceDataCPU.Value += m_DirtyPerInstanceMetadataSegmentList.TotalMemoryUsed;
		instanceDataCPU = counters.InstanceDataCPU;
		instanceDataCPU.Value += m_DirtyVisibilityInfoSegmentList.TotalMemoryUsed;
		m_DataAllocator.FillMemoryCounters(counters);
	}

	public NativeArray<float4>.ReadOnly GetCPUPersistentInstanceDataReadonly()
	{
		return m_CPUInstanceData.AsReadOnly();
	}

	public NativeArray<GPUDrivenPerInstanceMetadata>.ReadOnly GetPerInstanceMetadataReadonly()
	{
		return m_PerInstanceMetadata.AsReadOnly();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GPUDrivenAllocator.DataAllocation AllocateCustomData(int sizeInBytes)
	{
		return m_DataAllocator.AllocateCustomData(sizeInBytes);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FreeCustomData(in GPUDrivenAllocator.DataAllocation dataAllocation)
	{
		m_DataAllocator.FreeCustomData(in dataAllocation);
	}

	public void GrowInstanceCount(int instanceCount)
	{
		GrowOrCreateInstanceBuffers(instanceCount);
	}

	private void GrowOrCreateInstanceBuffers(int instanceCount)
	{
		int num = (m_InstanceAllocations.IsCreated ? m_InstanceAllocations.Length : 0);
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_InstanceAllocations, instanceCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		GPUDrivenAllocator.ClearInstanceAllocations(m_InstanceAllocations.GetSubArray(num, m_InstanceAllocations.Length - num));
		int num2 = (m_VisibilityInfoBuffer.IsCreated ? m_VisibilityInfoBuffer.Count : 0);
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_VisibilityInfo, instanceCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_VisibilityInfoBuffer.CreateOrResize(GraphicsBuffer.Target.Structured, instanceCount, UnsafeUtility.SizeOf<GPUDrivenVisibilityInfo>());
		m_DirtyVisibilityInfoSegmentList.Clear();
		if (num2 > 0)
		{
			m_DirtyVisibilityInfoSegmentList.AddItem(0, num2);
		}
		int num3 = (m_PerInstanceMetadataBuffer.IsCreated ? m_PerInstanceMetadataBuffer.Count : 0);
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_PerInstanceMetadata, instanceCount, Allocator.Persistent);
		m_PerInstanceMetadataBuffer.CreateOrResize(GraphicsBuffer.Target.Raw, instanceCount * UnsafeUtility.SizeOf<GPUDrivenPerInstanceMetadata>() / 4, 4);
		m_DirtyPerInstanceMetadataSegmentList.Clear();
		if (num3 > 0)
		{
			m_DirtyPerInstanceMetadataSegmentList.AddItem(0, num3);
		}
	}

	private void EnsureSufficientPersistentDataCapacity(in GPUDrivenAllocator.DataAllocation dataAllocation)
	{
		uint num = dataAllocation.TotalOffset();
		uint size = dataAllocation.Allocation.Size;
		uint num2 = num + size;
		int num3 = m_CPUInstanceData.Length * s_CPUInstanceDataStride;
		if (num2 > num3)
		{
			int size2 = (int)(num2 * 2);
			GrowOrCreatePersistentDataBuffers(size2);
		}
	}

	private void GrowOrCreatePersistentDataBuffers(int size)
	{
		size = Alignment.AlignUp(size, math.max(s_CPUInstanceDataStride, 4));
		int num = (m_CPUInstanceData.IsCreated ? (m_CPUInstanceData.Length * s_CPUInstanceDataStride) : 0);
		if (num > 0)
		{
			size = Alignment.AlignUp(size, num);
		}
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_CPUInstanceData, size / s_CPUInstanceDataStride, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_GPUPersistentInstanceData.CreateOrResize(GraphicsBuffer.Target.Raw, size / 4, 4);
		m_DirtyDataSegmentList.Clear();
		m_DirtyDataSegmentList.AddItem(0, num);
		if (num > 0)
		{
			m_BRG.ResourceRegistry.RecreateAllBatchIDs();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref GPUDrivenAllocator.DataAllocation GetInstanceDataAllocation(int index)
	{
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_InstanceAllocations, index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<GPUDrivenAllocator.DataAllocation>.ReadOnly GetInstanceDataAllocationsReadonly()
	{
		return m_InstanceAllocations.AsReadOnly();
	}

	public GPUDrivenAllocator.DataAllocation AllocateInstanceData(GPUDrivenIndexAllocator.IndexAllocation instanceIndex, int instanceSize)
	{
		GPUDrivenAllocator.DataAllocation dataAllocation = m_DataAllocator.AllocateInstanceData(instanceSize);
		if (!dataAllocation.IsValid())
		{
			return GPUDrivenAllocator.DataAllocation.Empty;
		}
		EnsureSufficientPersistentDataCapacity(in dataAllocation);
		m_InstanceAllocations[instanceIndex.Index] = dataAllocation;
		return dataAllocation;
	}

	public void FreeInstanceData(GPUDrivenIndexAllocator.IndexAllocation instanceIndex)
	{
		GPUDrivenAllocator.DataAllocation dataAllocation = m_InstanceAllocations[instanceIndex.Index];
		m_DataAllocator.FreeInstanceData(in dataAllocation);
		m_InstanceAllocations[instanceIndex.Index] = GPUDrivenAllocator.DataAllocation.Empty;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public unsafe GPUDrivenPropertyDataWriter BeginWritingProperties(bool autoMarkDirty = true)
	{
		return new GPUDrivenPropertyDataWriter(m_CPUInstanceData.GetUnsafePtr(), m_CPUInstanceData.Length * s_CPUInstanceDataStride, m_DirtyDataSegmentList, autoMarkDirty);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GPUDrivenPerInstanceMetadataWriter BeginWritingPerInstanceMetadata()
	{
		return new GPUDrivenPerInstanceMetadataWriter(m_PerInstanceMetadata, m_DirtyPerInstanceMetadataSegmentList);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<GPUDrivenVisibilityInfo>.ReadOnly GetAllVisibilityInfoReadonly()
	{
		return m_VisibilityInfo.AsReadOnly();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<GPUDrivenVisibilityInfo> GetAllVisibilityInfoRaw()
	{
		return m_VisibilityInfo;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref readonly GPUDrivenVisibilityInfo ReadVisibilityInfo(int index)
	{
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_VisibilityInfo, index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref GPUDrivenVisibilityInfo ModifyVisibilityInfo(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		m_DirtyVisibilityInfoSegmentList.AddItem(indexAllocation.Index, 1);
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_VisibilityInfo, indexAllocation.Index);
	}

	public NativeSparseSegmentList GetDirtyVisibilityInfoSegmentList()
	{
		return m_DirtyVisibilityInfoSegmentList;
	}

	private static void DisposeCollectionIfCreated<T>(ref NativeArray<T> array) where T : struct
	{
		if (array.IsCreated)
		{
			array.Dispose();
			array = default(NativeArray<T>);
		}
	}

	public PendingGPUUpload BeginGPUUpload()
	{
		using (new ProfilingScope(Profiling.UploadDebug))
		{
			GPUDrivenBRGDebug debugData = m_BRG.DebugData;
			if (debugData != null)
			{
				switch (debugData.GPUDataUploadDebugMode)
				{
				case GPUDrivenBRGDebug.DataUploadDebugMode.UploadFull:
					m_DirtyDataSegmentList.AddItem(0, GPUPersistentInstanceData.Count * GPUPersistentInstanceData.Stride);
					m_DirtyVisibilityInfoSegmentList.AddItem(0, m_VisibilityInfo.Length);
					m_DirtyPerInstanceMetadataSegmentList.AddItem(0, m_PerInstanceMetadata.Length);
					break;
				case GPUDrivenBRGDebug.DataUploadDebugMode.UploadManySmallSegments:
				{
					for (int i = 0; i < GPUPersistentInstanceData.Count; i += 1024)
					{
						m_DirtyDataSegmentList.AddItem(i * GPUPersistentInstanceData.Stride, GPUPersistentInstanceData.Stride);
					}
					break;
				}
				}
			}
		}
		PendingGPUUpload result = default(PendingGPUUpload);
		result.DirtyData = m_DirtyDataSegmentList.Merge(Allocator.TempJob);
		result.DirtyVisibilityInfo = m_DirtyVisibilityInfoSegmentList.Merge(Allocator.TempJob);
		result.DirtyPerInstanceMetadata = m_DirtyPerInstanceMetadataSegmentList.Merge(Allocator.TempJob);
		result.LODGroupData = m_LODGroupRepository.MergeDirtyDataSegments(Allocator.TempJob);
		return result;
	}

	public void CompleteGPUUpload(CommandBuffer cmd, ref PendingGPUUpload pendingGPUUpload)
	{
		using (new ProfilingScope(cmd, Profiling.UploadComplete))
		{
			using (new ProfilingScope(cmd, Profiling.UploadVisibilityInfo))
			{
				m_DataUploader.SetBufferDataSegments(cmd, m_VisibilityInfoBuffer.InternalBuffer, ref pendingGPUUpload.DirtyVisibilityInfo, m_VisibilityInfo, GPUDrivenUploaderSegmentUnit.Items);
			}
			using (new ProfilingScope(cmd, Profiling.UploadPerInstanceMetadata))
			{
				m_DataUploader.SetBufferDataSegments(cmd, m_PerInstanceMetadataBuffer.InternalBuffer, ref pendingGPUUpload.DirtyPerInstanceMetadata, m_PerInstanceMetadata, GPUDrivenUploaderSegmentUnit.Items);
			}
			using (new ProfilingScope(cmd, Profiling.UploadPersistentData))
			{
				m_DataUploader.SetBufferDataSegments(cmd, m_GPUPersistentInstanceData.InternalBuffer, ref pendingGPUUpload.DirtyData, m_CPUInstanceData, GPUDrivenUploaderSegmentUnit.Bytes);
			}
			using (new ProfilingScope(cmd, Profiling.UploadLODGroupData))
			{
				m_DataUploader.SetBufferDataSegments(cmd, m_LODGroupRepository.GPUDataBuffer, ref pendingGPUUpload.LODGroupData, m_LODGroupRepository.GetGroupData(), GPUDrivenUploaderSegmentUnit.Items);
			}
			cmd.SetGlobalBuffer(ShaderIDs._PerInstanceMetadata, m_PerInstanceMetadataBuffer.InternalBuffer);
			pendingGPUUpload.Dispose();
			m_DirtyDataSegmentList.Clear();
			m_DirtyVisibilityInfoSegmentList.Clear();
			m_DirtyPerInstanceMetadataSegmentList.Clear();
			m_LODGroupRepository.OnUploadedData();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public GPUDrivenAllocator.DataAllocation AllocateMaterialData(int sizeInBytes)
	{
		sizeInBytes = Alignment.AlignUp(sizeInBytes, 128);
		GPUDrivenAllocator.DataAllocation dataAllocation = m_DataAllocator.AllocateMaterialData(sizeInBytes);
		if (!dataAllocation.IsValid())
		{
			return GPUDrivenAllocator.DataAllocation.Empty;
		}
		EnsureSufficientPersistentDataCapacity(in dataAllocation);
		return dataAllocation;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void FreeMaterialData(GPUDrivenAllocator.DataAllocation dataAllocation)
	{
		m_DataAllocator.FreeMaterialData(in dataAllocation);
	}
}
