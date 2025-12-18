using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.ObjectTracking;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;

public class GPUDrivenLODViewCollection : IDisposable, IGPUDrivenMemoryProfilingSource
{
	public struct PendingBufferUpload
	{
		private NativeSparseSegmentList.SegmentMergeJobInfo m_SegmentMergeJobInfo;

		private NativeSparseSegmentList m_SegmentList;

		private readonly NativeArray<ViewDependentLODGroupData> m_CPUBuffer;

		private readonly GraphicsBuffer m_GPUBuffer;

		private readonly GPUDrivenBatchedDataUploader m_DataUploader;

		public PendingBufferUpload(NativeSparseSegmentList.SegmentMergeJobInfo segmentMergeJobInfo, NativeSparseSegmentList segmentList, NativeArray<ViewDependentLODGroupData> cpuBuffer, GraphicsBuffer gpuBuffer, GPUDrivenBatchedDataUploader dataUploader)
		{
			m_SegmentMergeJobInfo = segmentMergeJobInfo;
			m_SegmentList = segmentList;
			m_CPUBuffer = cpuBuffer;
			m_GPUBuffer = gpuBuffer;
			m_DataUploader = dataUploader;
		}

		public void Complete(CommandBuffer cmd)
		{
			m_SegmentMergeJobInfo.JobHandle.Complete();
			if (m_SegmentMergeJobInfo.Segments.Length > 0)
			{
				m_DataUploader.SetBufferDataSegments(cmd, m_GPUBuffer, ref m_SegmentMergeJobInfo, m_CPUBuffer, GPUDrivenUploaderSegmentUnit.Items);
			}
			m_SegmentMergeJobInfo.Dispose();
			m_SegmentList.Clear();
		}
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler UploadBuffers = new ProfilingSampler("UploadBuffers");
	}

	[BurstCompile]
	private struct FilterAnimatedGroupsJob : IJobFilter
	{
		[ReadOnly]
		public NativeArray<GPUDrivenLODGroupData> LODGroupData;

		[ReadOnly]
		public NativeArray<GPUDrivenLODGroupRepository.LODGroupMetadata> ProcessedLODGroups;

		public bool Execute(int index)
		{
			GPUDrivenLODGroupRepository.LODGroupMetadata lODGroupMetadata = ProcessedLODGroups[index];
			return LODGroupData[lODGroupMetadata.IndexAllocation.Index].AnimatedCrossFade != 0;
		}
	}

	[BurstCompile]
	private struct TickJob : IJobParallelFor
	{
		public const int kBatchSize = 16;

		public float DeltaTime;

		public float CrossFadeAnimationDuration;

		public GPUDrivenCullingContext.LODInfo LODInfo;

		[ReadOnly]
		public NativeArray<int> Indices;

		[ReadOnly]
		public NativeArray<GPUDrivenLODGroupRepository.LODGroupMetadata> ProcessedLODGroups;

		[ReadOnly]
		public NativeArray<GPUDrivenLODGroupData> LODGroupData;

		public NativeArray<ViewDependentLODGroupData> ViewLODGroupData;

		public NativeSparseSegmentList.ParallelWriter DirtySegmentList;

		public void Execute(int index)
		{
			if (index >= Indices.Length)
			{
				return;
			}
			int index2 = ProcessedLODGroups[Indices[index]].IndexAllocation.Index;
			GPUDrivenLODGroupData lodGroupData = LODGroupData[index2];
			ref ViewDependentLODGroupData reference = ref UnsafeCollectionExtensions.ElementAsRef(in ViewLODGroupData, index2);
			int num = GPUDrivenLODVisibility.ComputeDesiredLODIndex(in LODInfo, in lodGroupData);
			float lOD = reference.LOD;
			if (float.IsNaN(reference.LOD))
			{
				reference.LOD = num;
			}
			else
			{
				int num2 = math.abs((int)math.floor(reference.LOD));
				reference.LOD = Mathf.MoveTowards(math.abs(reference.LOD), num, DeltaTime / CrossFadeAnimationDuration);
				if (Mathf.Approximately(reference.LOD, num))
				{
					reference.LOD = num;
				}
				else if (num < num2)
				{
					reference.LOD = 0f - reference.LOD;
				}
			}
			if (!Mathf.Approximately(lOD, reference.LOD) || math.isnan(lOD) != math.isnan(reference.LOD))
			{
				DirtySegmentList.AddItemNoResize(index2, 1);
			}
		}
	}

	private struct View : IEquatable<View>, IDisposable
	{
		public ViewType ViewType;

		public Camera Camera;

		public Light Light;

		public NativeArray<ViewDependentLODGroupData> LODGroupData;

		public ResizableGraphicsBuffer LODGroupDataGPUBuffer;

		public NativeSparseSegmentList LODGroupDirtySegmentList;

		public JobHandle LastUpdateJobHandle;

		public int LastUpdatedFrameIndex;

		[CanBeNull]
		public Component GetComponentOrDefault()
		{
			return ViewType switch
			{
				ViewType.Camera => Camera, 
				ViewType.Light => Light, 
				_ => null, 
			};
		}

		public bool Equals(View other)
		{
			if (ViewType != other.ViewType)
			{
				return false;
			}
			return GetComponentOrDefault() == other.GetComponentOrDefault();
		}

		public override bool Equals(object obj)
		{
			if (obj is View other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			int hashCode2 = (int)ViewType;
			CombineHashCodeWithUnityObject(ref hashCode2, GetComponentOrDefault());
			return hashCode2;
			static void CombineHashCodeWithUnityObject(ref int hashCode, UnityEngine.Object obj)
			{
				hashCode = (hashCode * 397) ^ ((obj != null) ? obj.GetHashCode() : 0);
			}
		}

		public void Dispose()
		{
			LODGroupData.Dispose();
			LODGroupDataGPUBuffer.Dispose();
			LODGroupDirtySegmentList.Dispose();
		}
	}

	private enum ViewType
	{
		Invalid,
		Light,
		Camera
	}

	public struct ViewDependentLODGroupData
	{
		public float LOD;
	}

	private const byte kDataClearValue = byte.MaxValue;

	private readonly GPUDrivenBatchedDataUploader m_DataUploader;

	private readonly GPUDrivenLODGroupRepository m_Repository;

	private readonly Dictionary<BatchPackedCullingViewID, View> m_Views = new Dictionary<BatchPackedCullingViewID, View>();

	private int m_FrameIndex = -1;

	public GPUDrivenLODViewCollection(GPUDrivenLODGroupRepository repository, GPUDrivenBatchedDataUploader dataUploader)
	{
		m_Repository = repository;
		m_DataUploader = dataUploader;
	}

	public void Dispose()
	{
		foreach (View value in m_Views.Values)
		{
			value.Dispose();
		}
		m_Views.Clear();
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		foreach (View value2 in m_Views.Values)
		{
			counters.CollectBufferSize(counters.InstanceDataCPU, value2.LODGroupData);
			counters.CollectBufferSize(counters.InstanceDataGPU, value2.LODGroupDataGPUBuffer);
			ProfilerCounterValue<int> instanceDataCPU = counters.InstanceDataCPU;
			int value = instanceDataCPU.Value;
			NativeSparseSegmentList lODGroupDirtySegmentList = value2.LODGroupDirtySegmentList;
			instanceDataCPU.Value = value + lODGroupDirtySegmentList.TotalMemoryUsed;
		}
	}

	public JobHandle LaunchTickJob(BatchPackedCullingViewID viewID, in GPUDrivenCullingContext.LODInfo lodInfo, out NativeArray<ViewDependentLODGroupData>.ReadOnly viewData)
	{
		if (m_Views.TryGetValue(viewID, out var value))
		{
			JobHandle result;
			if (value.LastUpdatedFrameIndex >= m_FrameIndex)
			{
				result = ((value.LastUpdatedFrameIndex != m_FrameIndex) ? default(JobHandle) : value.LastUpdateJobHandle);
			}
			else
			{
				value.LastUpdateJobHandle.Complete();
				value.LastUpdateJobHandle = default(JobHandle);
				NativeArray<GPUDrivenLODGroupRepository.LODGroupMetadata> allMetadataValues = m_Repository.GetAllMetadataValues(Allocator.TempJob);
				NativeList<int> indices = new NativeList<int>(allMetadataValues.Length, Allocator.TempJob);
				value.LODGroupDirtySegmentList.Clear();
				value.LODGroupDirtySegmentList.EnsureCapacity(allMetadataValues.Length);
				FilterAnimatedGroupsJob jobData = default(FilterAnimatedGroupsJob);
				jobData.LODGroupData = m_Repository.GetGroupData();
				jobData.ProcessedLODGroups = allMetadataValues;
				result = jobData.ScheduleAppend(indices, allMetadataValues.Length);
				float deltaTime = ((Time.deltaTime > 0f) ? Time.smoothDeltaTime : 0f);
				TickJob jobData2 = default(TickJob);
				jobData2.DeltaTime = deltaTime;
				jobData2.CrossFadeAnimationDuration = LODGroup.crossFadeAnimationDuration;
				jobData2.LODInfo = lodInfo;
				jobData2.Indices = indices.AsDeferredJobArray();
				jobData2.ProcessedLODGroups = allMetadataValues;
				jobData2.LODGroupData = m_Repository.GetGroupData();
				jobData2.ViewLODGroupData = value.LODGroupData;
				jobData2.DirtySegmentList = value.LODGroupDirtySegmentList.AsParallelWriter();
				result = IJobParallelForExtensions.Schedule(jobData2, allMetadataValues.Length, 16, result);
				result = allMetadataValues.Dispose(result);
				result = (value.LastUpdateJobHandle = indices.Dispose(result));
			}
			value.LastUpdatedFrameIndex = m_FrameIndex;
			m_Views[viewID] = value;
			viewData = value.LODGroupData.AsReadOnly();
			return result;
		}
		viewData = default(NativeArray<ViewDependentLODGroupData>.ReadOnly);
		return default(JobHandle);
	}

	public void PreRender()
	{
		m_FrameIndex++;
		ClearDeletedViews();
	}

	public bool TryBeginBufferUpload(BatchPackedCullingViewID viewID, out PendingBufferUpload pendingBufferUpload)
	{
		using (new ProfilingScope(Profiling.UploadBuffers))
		{
			bool result = false;
			pendingBufferUpload = default(PendingBufferUpload);
			if (m_Views.TryGetValue(viewID, out var value))
			{
				value.LastUpdateJobHandle.Complete();
				value.LastUpdateJobHandle = default(JobHandle);
				if (value.LODGroupDirtySegmentList.Count > 0)
				{
					NativeSparseSegmentList.SegmentMergeJobInfo segmentMergeJobInfo = value.LODGroupDirtySegmentList.Merge(Allocator.TempJob);
					pendingBufferUpload = new PendingBufferUpload(segmentMergeJobInfo, value.LODGroupDirtySegmentList, value.LODGroupData, value.LODGroupDataGPUBuffer.InternalBuffer, m_DataUploader);
					result = true;
				}
				m_Views[viewID] = value;
			}
			return result;
		}
	}

	private void ClearDeletedViews()
	{
		NativeList<BatchPackedCullingViewID> nativeList = new NativeList<BatchPackedCullingViewID>(Allocator.Temp);
		foreach (KeyValuePair<BatchPackedCullingViewID, View> view in m_Views)
		{
			if (view.Value.GetComponentOrDefault() == null)
			{
				BatchPackedCullingViewID value = view.Key;
				nativeList.Add(in value);
			}
		}
		foreach (BatchPackedCullingViewID item in nativeList)
		{
			if (m_Views.Remove(item, out var value2))
			{
				value2.Dispose();
			}
		}
		nativeList.Dispose();
	}

	public void TryRegisterView(BatchPackedCullingViewID viewID)
	{
		if (m_Views.ContainsKey(viewID))
		{
			return;
		}
		Component component = ObjectDispatcherService.FindByInstanceId<Component>(viewID.GetInstanceID());
		if (component == null)
		{
			return;
		}
		View value = default(View);
		if (!(component is Camera camera))
		{
			if (!(component is Light light))
			{
				throw new ArgumentException($"Unsupported view object {component.name} ({component.GetType()}).", "viewID");
			}
			value.ViewType = ViewType.Light;
			value.Light = light;
		}
		else
		{
			value.ViewType = ViewType.Camera;
			value.Camera = camera;
		}
		int length = m_Repository.GetGroupData().Length;
		ResizeArrayAndClearNewItems(ref value.LODGroupData, length, byte.MaxValue);
		value.LODGroupDataGPUBuffer.CreateOrResize(GraphicsBuffer.Target.Raw, length, 4);
		value.LODGroupDirtySegmentList = new NativeSparseSegmentList(Allocator.Persistent, 4);
		value.LastUpdatedFrameIndex = -1;
		m_Views.Add(viewID, value);
	}

	public void ResizeLODGroupBuffers(int newCapacity)
	{
		NativeList<BatchPackedCullingViewID> allViewIDs = GetAllViewIDs(Allocator.Temp);
		foreach (BatchPackedCullingViewID item in allViewIDs)
		{
			View value = m_Views[item];
			ResizeArrayAndClearNewItems(ref value.LODGroupData, newCapacity, byte.MaxValue);
			value.LODGroupDataGPUBuffer.Resize(newCapacity);
			m_Views[item] = value;
		}
		allViewIDs.Dispose();
	}

	private unsafe static void ResizeArrayAndClearNewItems<T>(ref NativeArray<T> array, int newLength, byte clearValue) where T : unmanaged
	{
		if (!array.IsCreated || array.Length < newLength)
		{
			int num = (array.IsCreated ? array.Length : 0);
			NativeArray<T> destinationArray = new NativeArray<T>(newLength, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			if (num > 0)
			{
				destinationArray.CopyFrom(array, 0, num);
				array.Dispose();
			}
			UnsafeUtility.MemSet((byte*)destinationArray.GetUnsafePtr() + (nint)num * (nint)sizeof(T), clearValue, (newLength - num) * UnsafeUtility.SizeOf<T>());
			array = destinationArray;
		}
	}

	private NativeList<BatchPackedCullingViewID> GetAllViewIDs(Allocator allocator)
	{
		NativeList<BatchPackedCullingViewID> result = new NativeList<BatchPackedCullingViewID>(m_Views.Count, allocator);
		foreach (BatchPackedCullingViewID key in m_Views.Keys)
		{
			BatchPackedCullingViewID value = key;
			result.Add(in value);
		}
		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public void OnDestroyedLODGroup(int index)
	{
		foreach (View value in m_Views.Values)
		{
			View current = value;
			UnsafeCollectionExtensions.ElementAsRef(in current.LODGroupData, index) = new ViewDependentLODGroupData
			{
				LOD = float.NaN
			};
		}
	}

	[CanBeNull]
	public GraphicsBuffer GetViewDependentLODGroupDataOrDefault(BatchPackedCullingViewID viewID)
	{
		if (m_Views.TryGetValue(viewID, out var value))
		{
			return value.LODGroupDataGPUBuffer.InternalBuffer;
		}
		return null;
	}
}
