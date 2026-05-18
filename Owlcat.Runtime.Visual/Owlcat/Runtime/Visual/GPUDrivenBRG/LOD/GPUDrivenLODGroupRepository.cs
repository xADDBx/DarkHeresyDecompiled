using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Core.ObjectTracking;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Utilities;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;

public class GPUDrivenLODGroupRepository : IDisposable, IGPUDrivenMemoryProfilingSource
{
	[BurstCompile]
	private struct CollectExistingIndexAllocationsJob : IJobParallelForBatch
	{
		public const int kBatchSize = 32;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<EntityId> InstanceIDs;

		[ReadOnly]
		public NativeHashMap<int, LODGroupMetadata> Metadata;

		public NativeList<CollectedIndexAllocation>.ParallelWriter IndexAllocations;

		public unsafe void Execute(int startIndex, int count)
		{
			CollectedIndexAllocation* ptr = stackalloc CollectedIndexAllocation[32];
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				int num2 = startIndex + i;
				int key = InstanceIDs[num2];
				if (Metadata.TryGetValue(key, out var item))
				{
					ptr[num++] = new CollectedIndexAllocation
					{
						IndexAllocation = item.IndexAllocation,
						SourceIndex = num2,
						IsNew = false
					};
				}
			}
			if (num > 0)
			{
				IndexAllocations.AddRangeNoResize(ptr, num);
			}
		}
	}

	private struct CollectedIndexAllocation
	{
		public int SourceIndex;

		public GPUDrivenIndexAllocator.IndexAllocation IndexAllocation;

		public bool IsNew;
	}

	[BurstCompile]
	private struct UpdateLODGroupDataJob : IJobParallelFor
	{
		public const int kBatchSize = 16;

		public bool SupportDitheringCrossFade;

		public bool ForceUpdateWorldSpaceSize;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<CollectedIndexAllocation> CollectedIndexAllocations;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float> WorldSpaceSizes;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<LODFadeMode> FadeModes;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<Vector3> WorldSpaceReferencePoints;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> LODCounts;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> LODOffsets;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float> LODScreenRelativeTransitionHeights;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<float> LODFadeTransitionWidths;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<GPUDrivenLODGroupData> Data;

		[NativeDisableContainerSafetyRestriction]
		public NativeSparseSegmentList.ParallelWriter DirtySegmentList;

		public unsafe void Execute(int index)
		{
			if (index >= CollectedIndexAllocations.Length)
			{
				return;
			}
			CollectedIndexAllocation collectedIndexAllocation = CollectedIndexAllocations[index];
			int sourceIndex = collectedIndexAllocation.SourceIndex;
			GPUDrivenIndexAllocator.IndexAllocation indexAllocation = collectedIndexAllocation.IndexAllocation;
			LODFadeMode lODFadeMode = (SupportDitheringCrossFade ? FadeModes[sourceIndex] : LODFadeMode.None);
			if (lODFadeMode == LODFadeMode.SpeedTree)
			{
				lODFadeMode = LODFadeMode.CrossFade;
			}
			ref GPUDrivenLODGroupData reference = ref UnsafeCollectionExtensions.ElementAsRef(in Data, indexAllocation.Index);
			reference.WorldSpaceReferencePoint = WorldSpaceReferencePoints[sourceIndex];
			reference.LODCount = LODCounts[sourceIndex];
			reference.FadeMode = lODFadeMode;
			int num = LODOffsets[sourceIndex];
			float num2 = WorldSpaceSizes[sourceIndex];
			if (ForceUpdateWorldSpaceSize || collectedIndexAllocation.IsNew || !Mathf.Approximately(reference.WorldSpaceSize, num2))
			{
				reference.WorldSpaceSize = num2;
				for (int i = 0; i < reference.LODCount; i++)
				{
					int num3 = num + i;
					float num4 = LODScreenRelativeTransitionHeights[num3];
					float num5 = LODGroupRenderingUtils.CalculateLODDistance(num4, num2);
					reference.Distances[i] = num5;
					if (SupportDitheringCrossFade)
					{
						float num6 = ((i != 0) ? LODScreenRelativeTransitionHeights[num3 - 1] : 1f);
						float num7 = LODFadeTransitionWidths[num3];
						float relativeScreenHeight = num4 + num7 * (num6 - num4);
						float y = num5 - LODGroupRenderingUtils.CalculateLODDistance(relativeScreenHeight, num2);
						y = math.max(0f, y);
						reference.TransitionDistances[i] = y;
					}
					else
					{
						reference.TransitionDistances[i] = 0f;
					}
				}
			}
			DirtySegmentList.AddItemNoResize(indexAllocation.Index, 1);
		}
	}

	[BurstCompile]
	private struct UpdateAnimatedCrossFadeValuesJob : IJobParallelFor
	{
		public const int kBatchSize = 32;

		[ReadOnly]
		public NativeArray<CollectedIndexAllocation> CollectedIndexAllocations;

		[ReadOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeArray<int> AnimatedCrossFadeMask;

		[NativeDisableContainerSafetyRestriction]
		public NativeArray<GPUDrivenLODGroupData> Data;

		public void Execute(int index)
		{
			CollectedIndexAllocation collectedIndexAllocation = CollectedIndexAllocations[index];
			bool flag = false;
			if (AnimatedCrossFadeMask.IsCreated)
			{
				int sourceIndex = collectedIndexAllocation.SourceIndex;
				int index2 = sourceIndex / 32;
				int num = sourceIndex % 32;
				flag = (AnimatedCrossFadeMask[index2] & (1 << num)) != 0;
			}
			UnsafeCollectionExtensions.ElementAsRef(in Data, collectedIndexAllocation.IndexAllocation.Index).AnimatedCrossFade = (flag ? 1u : 0u);
		}
	}

	private static class Profiling
	{
		public static ProfilingSampler OnLODGroupTransformsChanged = new ProfilingSampler("OnLODGroupTransformsChanged");

		public static ProfilingSampler OnLODGroupsChanged = new ProfilingSampler("OnLODGroupsChanged");

		public static ProfilingSampler CollectIndexAllocations = new ProfilingSampler("CollectIndexAllocations");

		public static ProfilingSampler ProcessLODGroupChanges = new ProfilingSampler("ProcessLODGroupChanges");

		public static ProfilingSampler CollectAnimatedCrossFadeMaskSampler = new ProfilingSampler("CollectAnimatedCrossFadeMaskSampler");
	}

	public struct LODGroupMetadata : IEquatable<LODGroupMetadata>
	{
		public GPUDrivenIndexAllocator.IndexAllocation IndexAllocation;

		public int InstanceID;

		public bool Equals(LODGroupMetadata other)
		{
			if (IndexAllocation.Equals(other.IndexAllocation))
			{
				return InstanceID.Equals(other.InstanceID);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is LODGroupMetadata other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return IndexAllocation.GetHashCode() ^ InstanceID.GetHashCode();
		}
	}

	private class LODGroupTracker : ObjectTracker<LODGroup>, IObjectTransformTracker
	{
		private readonly GPUDrivenLODGroupRepository m_Repository;

		public LODGroupTracker(GPUDrivenLODGroupRepository repository)
			: base(ObjectDispatcherService.TypeTrackingFlags.Default)
		{
			m_Repository = repository;
		}

		public void ProcessTransformData(NativeArray<EntityId> transformedID, NativeArray<EntityId> parentID, NativeArray<Matrix4x4> localToWorldMatrices, NativeArray<Vector3> positions, NativeArray<Quaternion> rotations, NativeArray<Vector3> scales)
		{
			m_Repository.OnLODGroupTransformsChanged(transformedID);
		}

		public override void ProcessData(List<UnityEngine.Object> changed, NativeArray<EntityId> changedID, NativeArray<EntityId> destroyedID)
		{
			m_Repository.OnLODGroupsDestroyed(destroyedID);
			m_Repository.OnLODGroupsChanged(changedID);
		}
	}

	public const uint kForcedLODNone = uint.MaxValue;

	private const int kAnimatedCrossFadeMaskBits = 32;

	private readonly GPUDrivenBatchRendererGroup m_BRG;

	private readonly GPUDrivenProcessor m_GPUDrivenProcessor;

	private readonly bool m_SupportAnimatedLODCrossFade;

	private readonly GPUDrivenLODViewCollection m_ViewCollection;

	private NativeArray<GPUDrivenLODGroupData> m_Data;

	private NativeSparseSegmentList m_DirtyDataSegmentList;

	private ResizableGraphicsBuffer m_GPUData;

	private GPUDrivenIndexAllocator m_IndexAllocator;

	private LODGroupTracker m_LODGroupTracker;

	private NativeHashMap<int, LODGroupMetadata> m_Metadata;

	public GraphicsBuffer GPUDataBuffer => m_GPUData.InternalBuffer;

	public GPUDrivenLODGroupRepository(GPUDrivenBatchRendererGroup brg, GPUDrivenBRGSettings settings, GPUDrivenProcessor gpuDrivenProcessor, GPUDrivenBatchedDataUploader dataUploader)
	{
		m_BRG = brg;
		m_GPUDrivenProcessor = gpuDrivenProcessor;
		int num = settings.InitialInstanceCapacity / 4;
		m_LODGroupTracker = new LODGroupTracker(this);
		m_ViewCollection = new GPUDrivenLODViewCollection(this, dataUploader);
		m_Metadata = new NativeHashMap<int, LODGroupMetadata>(num, Allocator.Persistent);
		m_Data = new NativeArray<GPUDrivenLODGroupData>(num, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_GPUData = new ResizableGraphicsBuffer(GraphicsBuffer.Target.Structured, num, UnsafeUtility.SizeOf<GPUDrivenLODGroupData>());
		m_DirtyDataSegmentList = new NativeSparseSegmentList(Allocator.Persistent);
		m_IndexAllocator = new GPUDrivenIndexAllocator(num, autoGrow: true);
		ObjectDispatcherService.RegisterObjectTracker(m_LODGroupTracker);
		DitheringSettings ditheringSettings = WaaaghPipeline.Asset.DitheringSettings;
		m_SupportAnimatedLODCrossFade = ditheringSettings.EnableLODCrossFade && ditheringSettings.SupportAnimatedLODCrossFade;
	}

	public void Dispose()
	{
		ObjectDispatcherService.UnregisterObjectTracker(m_LODGroupTracker);
		m_ViewCollection.Dispose();
		m_LODGroupTracker = null;
		m_Metadata.Dispose();
		m_Metadata = default(NativeHashMap<int, LODGroupMetadata>);
		m_Data.Dispose();
		m_Data = default(NativeArray<GPUDrivenLODGroupData>);
		m_GPUData.Dispose();
		m_GPUData = default(ResizableGraphicsBuffer);
		m_DirtyDataSegmentList.Dispose();
		m_DirtyDataSegmentList = default(NativeSparseSegmentList);
		m_IndexAllocator.Dispose();
		m_IndexAllocator = null;
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		m_ViewCollection.FillMemoryCounters(counters);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_Data);
		counters.CollectBufferSize(counters.InstanceDataGPU, m_GPUData);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<LODGroupMetadata> GetAllMetadataValues(Allocator allocator)
	{
		return m_Metadata.GetValueArray(allocator);
	}

	public void PreRender()
	{
		m_ViewCollection.PreRender();
	}

	private void OnLODGroupsDestroyed(NativeArray<EntityId> instanceIDs)
	{
		if (instanceIDs.Length == 0)
		{
			return;
		}
		foreach (EntityId item2 in instanceIDs)
		{
			int key = item2;
			if (m_Metadata.TryGetValue(key, out var item))
			{
				m_Metadata.Remove(key);
				m_Data[item.IndexAllocation.Index] = default(GPUDrivenLODGroupData);
				m_ViewCollection.OnDestroyedLODGroup(item.IndexAllocation.Index);
			}
		}
	}

	public void OnMetViewID(BatchPackedCullingViewID viewID)
	{
		if (m_SupportAnimatedLODCrossFade)
		{
			m_ViewCollection.TryRegisterView(viewID);
		}
	}

	public JobHandle LaunchTickJobs(BatchPackedCullingViewID viewID, in GPUDrivenCullingContext.LODInfo lodInfo, out NativeArray<GPUDrivenLODViewCollection.ViewDependentLODGroupData>.ReadOnly viewData)
	{
		if (m_SupportAnimatedLODCrossFade)
		{
			return m_ViewCollection.LaunchTickJob(viewID, in lodInfo, out viewData);
		}
		viewData = default(NativeArray<GPUDrivenLODViewCollection.ViewDependentLODGroupData>.ReadOnly);
		return default(JobHandle);
	}

	private void OnLODGroupTransformsChanged(NativeArray<EntityId> instanceIDs)
	{
		if (instanceIDs.Length == 0)
		{
			return;
		}
		using (new ProfilingScope(Profiling.OnLODGroupTransformsChanged))
		{
			GPUDrivenProcessor.NativeLODGroupsData nativeLODGroupsData = m_GPUDrivenProcessor.DispatchLODGroupData(instanceIDs, Allocator.Temp);
			using (new ProfilingScope(Profiling.ProcessLODGroupChanges))
			{
				NativeList<CollectedIndexAllocation> nativeList = new NativeList<CollectedIndexAllocation>(nativeLODGroupsData.LODGroupID.Length, Allocator.TempJob);
				CollectExistingIndexAllocationsJob jobData = default(CollectExistingIndexAllocationsJob);
				jobData.IndexAllocations = nativeList.AsParallelWriter();
				jobData.InstanceIDs = nativeLODGroupsData.LODGroupID;
				jobData.Metadata = m_Metadata;
				JobHandle dependsOn = IJobParallelForBatchExtensions.Schedule(jobData, nativeLODGroupsData.LODGroupID.Length, 32);
				m_DirtyDataSegmentList.EnsureCapacity(m_DirtyDataSegmentList.Count + nativeLODGroupsData.LODGroupID.Length);
				UpdateLODGroupDataJob jobData2 = default(UpdateLODGroupDataJob);
				jobData2.SupportDitheringCrossFade = WaaaghPipeline.Asset.DitheringSettings.EnableLODCrossFade;
				jobData2.ForceUpdateWorldSpaceSize = false;
				jobData2.CollectedIndexAllocations = nativeList.AsDeferredJobArray();
				jobData2.FadeModes = nativeLODGroupsData.FadeMode;
				jobData2.WorldSpaceSizes = nativeLODGroupsData.WorldSpaceSize;
				jobData2.LODCounts = nativeLODGroupsData.LODCount;
				jobData2.LODOffsets = nativeLODGroupsData.LODOffset;
				jobData2.WorldSpaceReferencePoints = nativeLODGroupsData.WorldSpaceReferencePoint.Reinterpret<Vector3>();
				jobData2.LODFadeTransitionWidths = nativeLODGroupsData.LODFadeTransitionWidth;
				jobData2.LODScreenRelativeTransitionHeights = nativeLODGroupsData.LODScreenRelativeTransitionHeight;
				jobData2.Data = m_Data;
				jobData2.DirtySegmentList = m_DirtyDataSegmentList.AsParallelWriter();
				dependsOn = IJobParallelForExtensions.Schedule(jobData2, nativeLODGroupsData.LODGroupID.Length, 16, dependsOn);
				nativeList.Dispose(dependsOn);
				dependsOn.Complete();
			}
			nativeLODGroupsData.Dispose();
		}
	}

	private void OnLODGroupsChanged(NativeArray<EntityId> instanceIDs)
	{
		if (instanceIDs.Length == 0)
		{
			return;
		}
		using (new ProfilingScope(Profiling.OnLODGroupsChanged))
		{
			GPUDrivenProcessor.NativeLODGroupsData nativeLODGroupsData = m_GPUDrivenProcessor.DispatchLODGroupData(instanceIDs, Allocator.Temp);
			OnLODGroupsDestroyed(nativeLODGroupsData.InvalidLODGroupID);
			using (new ProfilingScope(Profiling.ProcessLODGroupChanges))
			{
				DitheringSettings ditheringSettings = WaaaghPipeline.Asset.DitheringSettings;
				bool enableLODCrossFade = ditheringSettings.EnableLODCrossFade;
				NativeArray<CollectedIndexAllocation> collectedIndexAllocations = new NativeArray<CollectedIndexAllocation>(nativeLODGroupsData.LODGroupID.Length, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
				using (new ProfilingScope(Profiling.CollectIndexAllocations))
				{
					for (int i = 0; i < nativeLODGroupsData.LODGroupID.Length; i++)
					{
						int instanceID = nativeLODGroupsData.LODGroupID[i];
						collectedIndexAllocations[i] = new CollectedIndexAllocation
						{
							IndexAllocation = EnsureAllocated(instanceID, out var isNew),
							IsNew = isNew,
							SourceIndex = i
						};
					}
				}
				m_DirtyDataSegmentList.EnsureCapacity(m_DirtyDataSegmentList.Count + nativeLODGroupsData.LODGroupID.Length);
				UpdateLODGroupDataJob jobData = default(UpdateLODGroupDataJob);
				jobData.SupportDitheringCrossFade = enableLODCrossFade;
				jobData.ForceUpdateWorldSpaceSize = true;
				jobData.CollectedIndexAllocations = collectedIndexAllocations;
				jobData.FadeModes = nativeLODGroupsData.FadeMode;
				jobData.WorldSpaceSizes = nativeLODGroupsData.WorldSpaceSize;
				jobData.LODCounts = nativeLODGroupsData.LODCount;
				jobData.LODOffsets = nativeLODGroupsData.LODOffset;
				jobData.WorldSpaceReferencePoints = nativeLODGroupsData.WorldSpaceReferencePoint.Reinterpret<Vector3>();
				jobData.LODFadeTransitionWidths = nativeLODGroupsData.LODFadeTransitionWidth;
				jobData.LODScreenRelativeTransitionHeights = nativeLODGroupsData.LODScreenRelativeTransitionHeight;
				jobData.Data = m_Data;
				jobData.DirtySegmentList = m_DirtyDataSegmentList.AsParallelWriter();
				JobHandle jobHandle = IJobParallelForExtensions.Schedule(jobData, nativeLODGroupsData.LODGroupID.Length, 16);
				JobHandle.ScheduleBatchedJobs();
				NativeArray<int> animatedCrossFadeMask = (ditheringSettings.SupportAnimatedLODCrossFade ? CollectAnimatedCrossFadeMask(nativeLODGroupsData.LODGroupID, Allocator.TempJob) : default(NativeArray<int>));
				UpdateAnimatedCrossFadeValuesJob jobData2 = default(UpdateAnimatedCrossFadeValuesJob);
				jobData2.Data = m_Data;
				jobData2.CollectedIndexAllocations = collectedIndexAllocations;
				jobData2.AnimatedCrossFadeMask = animatedCrossFadeMask;
				IJobParallelForExtensions.Schedule(jobData2, nativeLODGroupsData.LODGroupID.Length, 32).Complete();
				jobHandle.Complete();
				if (animatedCrossFadeMask.IsCreated)
				{
					animatedCrossFadeMask.Dispose();
				}
				collectedIndexAllocations.Dispose();
				nativeLODGroupsData.Dispose();
			}
		}
	}

	private static NativeArray<int> CollectAnimatedCrossFadeMask(NativeArray<EntityId> instanceIDs, Allocator allocator)
	{
		using (new ProfilingScope(Profiling.CollectAnimatedCrossFadeMaskSampler))
		{
			NativeArray<int> nativeArray = new NativeArray<int>(Alignment.AlignUp(instanceIDs.Length, 32) / 32, allocator, NativeArrayOptions.UninitializedMemory);
			for (int i = 0; i < instanceIDs.Length; i++)
			{
				bool animateCrossFading = ObjectDispatcherService.FindByInstanceId<LODGroup>(instanceIDs[i]).animateCrossFading;
				ref int reference = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, i / 32);
				if (animateCrossFading)
				{
					reference |= 1 << i % 32;
				}
				else
				{
					reference &= ~(1 << i % 32);
				}
			}
			return nativeArray;
		}
	}

	private GPUDrivenIndexAllocator.IndexAllocation EnsureAllocated(int instanceID, out bool isNew)
	{
		if (!m_Metadata.TryGetValue(instanceID, out var item))
		{
			GPUDrivenIndexAllocator.IndexAllocation indexAllocation = m_IndexAllocator.Allocate();
			if (indexAllocation.Index >= m_Data.Length)
			{
				NativeArray<GPUDrivenLODGroupData> nativeArray = new NativeArray<GPUDrivenLODGroupData>(m_IndexAllocator.Capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
				NativeArray<GPUDrivenLODGroupData>.Copy(m_Data, nativeArray, m_Data.Length);
				m_GPUData.Resize(nativeArray.Length);
				m_DirtyDataSegmentList.Clear();
				m_DirtyDataSegmentList.AddItem(0, m_Data.Length);
				m_Data.Dispose();
				m_Data = nativeArray;
				m_ViewCollection.ResizeLODGroupBuffers(nativeArray.Length);
			}
			LODGroupMetadata lODGroupMetadata = default(LODGroupMetadata);
			lODGroupMetadata.IndexAllocation = indexAllocation;
			lODGroupMetadata.InstanceID = instanceID;
			item = lODGroupMetadata;
			m_Metadata.Add(instanceID, item);
			m_Data[indexAllocation.Index] = new GPUDrivenLODGroupData
			{
				SelectionForcedLOD = uint.MaxValue
			};
			isNew = true;
		}
		else
		{
			isNew = false;
		}
		return item.IndexAllocation;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<GPUDrivenLODGroupData> GetGroupData()
	{
		return m_Data;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeHashMap<int, LODGroupMetadata>.ReadOnly GetGroupMetadataReadonly()
	{
		return m_Metadata.AsReadOnly();
	}

	public NativeSparseSegmentList.SegmentMergeJobInfo MergeDirtyDataSegments(Allocator allocator)
	{
		return m_DirtyDataSegmentList.Merge(allocator);
	}

	public void OnUploadedData()
	{
		m_DirtyDataSegmentList.Clear();
	}

	public bool TryBeginBufferUpload(BatchPackedCullingViewID viewID, out GPUDrivenLODViewCollection.PendingBufferUpload pendingBufferUpload)
	{
		return m_ViewCollection.TryBeginBufferUpload(viewID, out pendingBufferUpload);
	}

	[CanBeNull]
	public GraphicsBuffer GetViewDependentLODGroupDataOrDefault(BatchPackedCullingViewID viewID)
	{
		return m_ViewCollection.GetViewDependentLODGroupDataOrDefault(viewID);
	}
}
