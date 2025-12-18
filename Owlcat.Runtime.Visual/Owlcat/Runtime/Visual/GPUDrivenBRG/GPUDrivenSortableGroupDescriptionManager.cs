using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Collections.Extensions;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;
using Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

internal class GPUDrivenSortableGroupDescriptionManager
{
	private static class GroupSortingAlgorithmSelector
	{
		public static JobHandle Small(NativeArray<SortableGroupDescription> sortableGroupDescriptions, GPUDrivenBRGMaterialMergeMode materialMergeMode, JobHandle inputDeps)
		{
			switch (materialMergeMode)
			{
			case GPUDrivenBRGMaterialMergeMode.Disabled:
				return inputDeps;
			case GPUDrivenBRGMaterialMergeMode.CoarseSort:
				return sortableGroupDescriptions.SortJob(default(CoarseSortableGroupDescriptionComparer)).Schedule();
			case GPUDrivenBRGMaterialMergeMode.SegmentSort:
			case GPUDrivenBRGMaterialMergeMode.FineSort:
				return sortableGroupDescriptions.SortJob(new SortableGroupDescriptionComparer
				{
					Approximate = true
				}).Schedule(inputDeps);
			case GPUDrivenBRGMaterialMergeMode.ExactSort:
				return sortableGroupDescriptions.SortJob(new SortableGroupDescriptionComparer
				{
					Approximate = false
				}).Schedule(inputDeps);
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public static JobHandle Big(NativeArray<SortableGroupDescription> sortableGroupDescriptions, GPUDrivenBRGMaterialMergeMode materialMergeMode, JobHandle inputDeps)
		{
			JobHandle jobHandle = inputDeps;
			switch (materialMergeMode)
			{
			case GPUDrivenBRGMaterialMergeMode.CoarseSort:
			{
				CustomParallelSortJob<SortableGroupDescription, CoarseSortableGroupDescriptionComparer> customParallelSortJob2 = default(CustomParallelSortJob<SortableGroupDescription, CoarseSortableGroupDescriptionComparer>);
				customParallelSortJob2.Array = sortableGroupDescriptions;
				customParallelSortJob2.Comparer = default(CoarseSortableGroupDescriptionComparer);
				jobHandle = customParallelSortJob2.Schedule(jobHandle);
				break;
			}
			case GPUDrivenBRGMaterialMergeMode.SegmentSort:
			{
				SortableGroupDescriptionComparer sortableGroupDescriptionComparer = default(SortableGroupDescriptionComparer);
				sortableGroupDescriptionComparer.Approximate = true;
				SortableGroupDescriptionComparer comparer = sortableGroupDescriptionComparer;
				NativeList<JobHandle> nativeList = new NativeList<JobHandle>((int)math.ceil((float)sortableGroupDescriptions.Length / 250f), Allocator.Temp);
				for (int i = 0; i < sortableGroupDescriptions.Length; i += 250)
				{
					int length = math.min(250, sortableGroupDescriptions.Length - i);
					NativeArray<SortableGroupDescription> subArray = sortableGroupDescriptions.GetSubArray(i, length);
					JobHandle value = new CustomParallelSortJob<SortableGroupDescription, SortableGroupDescriptionComparer>
					{
						Array = subArray,
						Comparer = comparer
					}.Schedule(jobHandle);
					nativeList.Add(in value);
				}
				jobHandle = JobHandle.CombineDependencies(nativeList.AsArray());
				break;
			}
			case GPUDrivenBRGMaterialMergeMode.FineSort:
			{
				CustomParallelSortJob<SortableGroupDescription, SortableGroupDescriptionComparer> customParallelSortJob = default(CustomParallelSortJob<SortableGroupDescription, SortableGroupDescriptionComparer>);
				customParallelSortJob.Array = sortableGroupDescriptions;
				customParallelSortJob.Comparer = new SortableGroupDescriptionComparer
				{
					Approximate = true
				};
				jobHandle = customParallelSortJob.Schedule(jobHandle);
				break;
			}
			case GPUDrivenBRGMaterialMergeMode.ExactSort:
			{
				CustomParallelSortJob<SortableGroupDescription, SortableGroupDescriptionComparer> customParallelSortJob = default(CustomParallelSortJob<SortableGroupDescription, SortableGroupDescriptionComparer>);
				customParallelSortJob.Array = sortableGroupDescriptions;
				customParallelSortJob.Comparer = new SortableGroupDescriptionComparer
				{
					Approximate = false
				};
				jobHandle = customParallelSortJob.Schedule(jobHandle);
				break;
			}
			default:
				throw new ArgumentOutOfRangeException();
			case GPUDrivenBRGMaterialMergeMode.Disabled:
				break;
			}
			return jobHandle;
		}
	}

	[BurstCompile]
	public struct StripRedundantDataFromSortableGroupsJob : IJobParallelFor
	{
		public const int kBatchSize = 32;

		public GPUDrivenRendererGroupPool.ViewType ViewType;

		public NativeArray<SortableGroupDescription> SortableGroupDescriptions;

		public void Execute(int index)
		{
			ref SortableGroupDescription reference = ref UnsafeCollectionExtensions.ElementAsRef(in SortableGroupDescriptions, index);
			ref GPUDrivenMaterialUniquenessKey gPUDrivenMaterialUniquenessKey = ref reference.GPUDrivenMaterialUniquenessKey;
			ref GPUDrivenRendererGroupPool.RendererSettings rendererSettings = ref reference.RendererSettings;
			GPUDrivenRendererGroupPool.ViewType viewType = ViewType;
			if (viewType == GPUDrivenRendererGroupPool.ViewType.Shadows || viewType == GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped)
			{
				int enabledPassesMask = 1 << gPUDrivenMaterialUniquenessKey.ShadowCasterPassIndex;
				gPUDrivenMaterialUniquenessKey.EnabledPassesMask = enabledPassesMask;
				gPUDrivenMaterialUniquenessKey.EnabledKeywordsMask = gPUDrivenMaterialUniquenessKey.EnabledShadowCasterKeywordsMask;
				gPUDrivenMaterialUniquenessKey.IgnoredBatchBreakingProperties = gPUDrivenMaterialUniquenessKey.IgnoredShadowCasterBatchBreakingProperties;
				gPUDrivenMaterialUniquenessKey.BatchBreakingPropertiesHashCode = gPUDrivenMaterialUniquenessKey.ShadowCasterBreakingPropertiesHashCode;
				rendererSettings.MotionVectorGenerationMode = MotionVectorGenerationMode.Camera;
				rendererSettings.Flags &= ~(GPUDrivenRendererGroupPool.RendererGroupFlags.ReceiveShadows | GPUDrivenRendererGroupPool.RendererGroupFlags.AffectsLightMaps);
			}
			else if (ViewType == GPUDrivenRendererGroupPool.ViewType.DepthOnly)
			{
				int enabledPassesMask2 = 1 << gPUDrivenMaterialUniquenessKey.DepthOnlyPassIndex;
				gPUDrivenMaterialUniquenessKey.EnabledPassesMask = enabledPassesMask2;
				gPUDrivenMaterialUniquenessKey.EnabledKeywordsMask = gPUDrivenMaterialUniquenessKey.EnabledDepthOnlyKeywordsMask;
				gPUDrivenMaterialUniquenessKey.IgnoredBatchBreakingProperties = gPUDrivenMaterialUniquenessKey.IgnoredDepthOnlyBreakingProperties;
				gPUDrivenMaterialUniquenessKey.BatchBreakingPropertiesHashCode = gPUDrivenMaterialUniquenessKey.DepthOnlyBreakingPropertiesHashCode;
				rendererSettings.MotionVectorGenerationMode = MotionVectorGenerationMode.Camera;
				rendererSettings.ShadowCastingMode = ShadowCastingMode.Off;
				rendererSettings.Flags &= ~(GPUDrivenRendererGroupPool.RendererGroupFlags.StaticShadowCater | GPUDrivenRendererGroupPool.RendererGroupFlags.ReceiveShadows | GPUDrivenRendererGroupPool.RendererGroupFlags.AffectsLightMaps);
			}
			else if (ViewType == GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors)
			{
				int enabledPassesMask3 = 1 << gPUDrivenMaterialUniquenessKey.MotionVectorsPassIndex;
				gPUDrivenMaterialUniquenessKey.EnabledPassesMask = enabledPassesMask3;
				rendererSettings.ShadowCastingMode = ShadowCastingMode.Off;
				rendererSettings.Flags &= ~(GPUDrivenRendererGroupPool.RendererGroupFlags.StaticShadowCater | GPUDrivenRendererGroupPool.RendererGroupFlags.ReceiveShadows | GPUDrivenRendererGroupPool.RendererGroupFlags.AffectsLightMaps);
			}
			else
			{
				rendererSettings.MotionVectorGenerationMode = MotionVectorGenerationMode.Camera;
				rendererSettings.ShadowCastingMode = ShadowCastingMode.Off;
				rendererSettings.Flags &= ~(GPUDrivenRendererGroupPool.RendererGroupFlags.StaticShadowCater | GPUDrivenRendererGroupPool.RendererGroupFlags.AffectsLightMaps);
			}
		}
	}

	[BurstCompile]
	private struct CollectSortableGroupDescriptionsJob : IJobParallelForBatch
	{
		public const int kBatchSize = 32;

		public GPUDrivenRendererGroupPool.ViewType ViewType;

		[ReadOnly]
		public NativeArray<GPUDrivenIndexAllocator.IndexAllocation> GroupAllocations;

		[ReadOnly]
		public NativeArray<GPUDrivenRendererGroupPool.RendererGroup>.ReadOnly RendererGroups;

		[ReadOnly]
		public NativeArray<GPUDrivenResourceRegistry.UnmanagedMaterialInfo>.ReadOnly Materials;

		[ReadOnly]
		public NativeArray<UnsafeList<int>> RendererGroupIndices;

		public NativeList<SortableGroupDescription>.ParallelWriter SortableGroupDescriptions;

		[NativeDisableContainerSafetyRestriction]
		public NativeList<GPUDrivenIndexAllocator.IndexAllocation>.ParallelWriter EmptyGroupIndices;

		public unsafe void Execute(int startIndex, int count)
		{
			GPUDrivenIndexAllocator.IndexAllocation* ptr = stackalloc GPUDrivenIndexAllocator.IndexAllocation[32];
			int num = 0;
			SortableGroupDescription* ptr2 = stackalloc SortableGroupDescription[32];
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				int index = startIndex + i;
				GPUDrivenIndexAllocator.IndexAllocation indexAllocation = GroupAllocations[index];
				GPUDrivenRendererGroupPool.RendererGroup rendererGroup = RendererGroups[indexAllocation.Index];
				GPUDrivenRendererGroupPool.RendererSettings rendererSettings = rendererGroup.Key.RendererSettings;
				if ((rendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.Disabled) != 0)
				{
					continue;
				}
				GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo = Materials[rendererGroup.Key.MaterialAllocation.Index];
				if (RendererGroupIndices[indexAllocation.Index].Length == 0)
				{
					if (EmptyGroupIndices.ListData != null && EmptyGroupIndices.Ptr != null)
					{
						ptr[num++] = indexAllocation;
					}
					continue;
				}
				GPUDrivenMaterialUniquenessKey materialUniquenessKey = unmanagedMaterialInfo.GPUDrivenMaterialUniquenessKey;
				if (ApplyFilter(unmanagedMaterialInfo, ref materialUniquenessKey, ref rendererSettings))
				{
					ptr2[num2++] = new SortableGroupDescription
					{
						IndexAllocation = indexAllocation,
						GPUDrivenMaterialUniquenessKey = materialUniquenessKey,
						MeshIndex = rendererGroup.Key.MeshAllocation.Index,
						MaterialFlags = unmanagedMaterialInfo.Flags,
						RendererSettings = rendererSettings
					};
				}
			}
			if (num > 0 && EmptyGroupIndices.Ptr != null)
			{
				EmptyGroupIndices.AddRangeNoResize(ptr, num);
			}
			if (num2 > 0)
			{
				SortableGroupDescriptions.AddRangeNoResize(ptr2, num2);
			}
		}

		private bool ApplyFilter(GPUDrivenResourceRegistry.UnmanagedMaterialInfo unmanagedMaterialInfo, ref GPUDrivenMaterialUniquenessKey materialUniquenessKey, ref GPUDrivenRendererGroupPool.RendererSettings rendererSettings)
		{
			if (ViewType == GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped && (rendererSettings.Flags & GPUDrivenRendererGroupPool.RendererGroupFlags.AffectsLightMaps) != 0)
			{
				return false;
			}
			GPUDrivenRendererGroupPool.ViewType viewType = ViewType;
			if (viewType == GPUDrivenRendererGroupPool.ViewType.Shadows || viewType == GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped)
			{
				if (rendererSettings.ShadowCastingMode == ShadowCastingMode.Off)
				{
					return false;
				}
				if (materialUniquenessKey.ShadowCasterPassIndex < 0)
				{
					return false;
				}
				int num = 1 << materialUniquenessKey.ShadowCasterPassIndex;
				if ((materialUniquenessKey.EnabledPassesMask & num) == 0)
				{
					return false;
				}
			}
			else if (ViewType == GPUDrivenRendererGroupPool.ViewType.DepthOnly)
			{
				if ((unmanagedMaterialInfo.Flags & GPUDrivenResourceRegistry.MaterialFlags.Transparent) != 0 && (unmanagedMaterialInfo.Flags & GPUDrivenResourceRegistry.MaterialFlags.OpaqueDistortion) == 0)
				{
					return false;
				}
				if ((materialUniquenessKey.ShaderTypeMask & GPUDrivenShaderMetadata.TypeFlags.DecalAny) != 0)
				{
					return false;
				}
				if (materialUniquenessKey.DepthOnlyPassIndex < 0)
				{
					return false;
				}
				int num2 = 1 << materialUniquenessKey.DepthOnlyPassIndex;
				if ((materialUniquenessKey.EnabledPassesMask & num2) == 0)
				{
					return false;
				}
			}
			else if (ViewType == GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors)
			{
				if ((unmanagedMaterialInfo.Flags & GPUDrivenResourceRegistry.MaterialFlags.Transparent) != 0)
				{
					return false;
				}
				if ((materialUniquenessKey.ShaderTypeMask & GPUDrivenShaderMetadata.TypeFlags.DecalAny) != 0)
				{
					return false;
				}
				if (materialUniquenessKey.MotionVectorsPassIndex < 0)
				{
					return false;
				}
				if (!materialUniquenessKey.HasMotionVectorsPassEnabled() && rendererSettings.MotionVectorGenerationMode == MotionVectorGenerationMode.Camera)
				{
					return false;
				}
			}
			else if (rendererSettings.ShadowCastingMode == ShadowCastingMode.ShadowsOnly)
			{
				return false;
			}
			return true;
		}
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	[BurstCompile]
	private struct CoarseSortableGroupDescriptionComparer : IComparer<SortableGroupDescription>
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(SortableGroupDescription x, SortableGroupDescription y)
		{
			int num = x.GPUDrivenMaterialUniquenessKey.ShaderInstanceID.CompareTo(y.GPUDrivenMaterialUniquenessKey.ShaderInstanceID);
			if (num != 0)
			{
				return num;
			}
			int num2 = x.GPUDrivenMaterialUniquenessKey.EnabledKeywordsMask.CompareTo(y.GPUDrivenMaterialUniquenessKey.EnabledKeywordsMask);
			if (num2 != 0)
			{
				return num2;
			}
			return x.MeshIndex.CompareTo(y.MeshIndex);
		}
	}

	[BurstCompile]
	private struct SortableGroupDescriptionComparer : IComparer<SortableGroupDescription>
	{
		public bool Approximate;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public int Compare(SortableGroupDescription x, SortableGroupDescription y)
		{
			return x.CompareTo(y, Approximate);
		}
	}

	[BurstCompile]
	public struct SortableGroupDescription
	{
		public GPUDrivenIndexAllocator.IndexAllocation IndexAllocation;

		public GPUDrivenMaterialUniquenessKey GPUDrivenMaterialUniquenessKey;

		public int MeshIndex;

		public GPUDrivenResourceRegistry.MaterialFlags MaterialFlags;

		public GPUDrivenRendererGroupPool.RendererSettings RendererSettings;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool AllowMerging()
		{
			return GPUDrivenMaterialUniquenessKey.RenderType != GPUDrivenRenderingUtils.RenderType.Various;
		}

		public int CompareTo(SortableGroupDescription other, bool approximate = false)
		{
			int num = RendererSettings.CompareTo(other.RendererSettings);
			if (num != 0)
			{
				return num;
			}
			int materialFlags = (int)MaterialFlags;
			int num2 = materialFlags.CompareTo((int)other.MaterialFlags);
			if (num2 != 0)
			{
				return num2;
			}
			int num3 = GPUDrivenMaterialUniquenessKey.CompareTo(other.GPUDrivenMaterialUniquenessKey, approximate);
			if (num3 != 0)
			{
				return num3;
			}
			return MeshIndex.CompareTo(other.MeshIndex);
		}
	}

	public struct GetValueArrayJobInfo<TValue> : IDisposable where TValue : struct
	{
		public NativeArray<TValue> Values;

		public JobHandle JobHandle;

		public bool IsCreated => Values.IsCreated;

		public void Dispose()
		{
			Values.Dispose();
		}
	}

	public struct JobInfo
	{
		[StructLayout(LayoutKind.Sequential, Size = 1)]
		public struct ExpectedJobDurationComparer : IComparer<JobInfo>
		{
			public int Compare(JobInfo x, JobInfo y)
			{
				int durationLabel = GetDurationLabel(x.ViewType);
				int durationLabel2 = GetDurationLabel(y.ViewType);
				return durationLabel.CompareTo(durationLabel2);
			}

			[MethodImpl(MethodImplOptions.AggressiveInlining)]
			private static int GetDurationLabel(GPUDrivenRendererGroupPool.ViewType viewType)
			{
				return viewType switch
				{
					GPUDrivenRendererGroupPool.ViewType.Camera => 10, 
					GPUDrivenRendererGroupPool.ViewType.DepthOnly => 8, 
					GPUDrivenRendererGroupPool.ViewType.Shadows => 6, 
					GPUDrivenRendererGroupPool.ViewType.CameraMotionVectors => 3, 
					GPUDrivenRendererGroupPool.ViewType.ShadowsCullLightmapped => 1, 
					_ => throw new ArgumentOutOfRangeException("viewType", viewType, null), 
				};
			}
		}

		public JobHandle CollectJob;

		public GPUDrivenRendererGroupPool.ViewType ViewType;

		public NativeList<SortableGroupDescription> Descriptions;

		public void Dispose(JobHandle jobHandle)
		{
			Descriptions.Dispose(jobHandle);
		}
	}

	private readonly GPUDrivenRendererGroupPool m_RendererGroupPool;

	private readonly GPUDrivenResourceRegistry m_ResourceRegistry;

	public GPUDrivenSortableGroupDescriptionManager(GPUDrivenRendererGroupPool rendererGroupPool, GPUDrivenResourceRegistry resourceRegistry)
	{
		m_RendererGroupPool = rendererGroupPool;
		m_ResourceRegistry = resourceRegistry;
	}

	public JobInfo Collect(GPUDrivenRendererGroupPool.ViewType viewType, NativeList<GPUDrivenIndexAllocator.IndexAllocation> emptyGroupIndices, ref GetValueArrayJobInfo<GPUDrivenIndexAllocator.IndexAllocation> groupAllocationsValueArray, AllocatorManager.AllocatorHandle allocator)
	{
		NativeHashMap<GPUDrivenRendererGroupPool.RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation> rendererGroupKeyToIndex = m_RendererGroupPool.GetRendererGroupKeyToIndex();
		NativeList<SortableGroupDescription> descriptions = new NativeList<SortableGroupDescription>(rendererGroupKeyToIndex.Count, allocator);
		if (!groupAllocationsValueArray.Values.IsCreated)
		{
			groupAllocationsValueArray.Values = new NativeArray<GPUDrivenIndexAllocator.IndexAllocation>(rendererGroupKeyToIndex.Count, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
			groupAllocationsValueArray.JobHandle = new UnsafeHashMapExtensions.GetHashMapValueArray<GPUDrivenRendererGroupPool.RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation>.Job
			{
				HashMap = rendererGroupKeyToIndex,
				Result = groupAllocationsValueArray.Values
			}.Schedule();
			JobHandle.ScheduleBatchedJobs();
		}
		JobHandle jobHandle = groupAllocationsValueArray.JobHandle;
		CollectSortableGroupDescriptionsJob jobData = default(CollectSortableGroupDescriptionsJob);
		jobData.ViewType = viewType;
		jobData.GroupAllocations = groupAllocationsValueArray.Values;
		jobData.RendererGroups = m_RendererGroupPool.GetInnerPool();
		jobData.Materials = m_ResourceRegistry.GetInnerUnmanagedMaterialPool();
		jobData.RendererGroupIndices = m_RendererGroupPool.GetRendererGroupIndices();
		jobData.SortableGroupDescriptions = descriptions.AsParallelWriter();
		jobData.EmptyGroupIndices = emptyGroupIndices.AsParallelWriter();
		jobHandle = IJobParallelForBatchExtensions.Schedule(jobData, groupAllocationsValueArray.Values.Length, 32, jobHandle);
		JobInfo result = default(JobInfo);
		result.Descriptions = descriptions;
		result.CollectJob = jobHandle;
		result.ViewType = viewType;
		return result;
	}

	public static JobHandle SortGroupDescriptions(NativeArray<SortableGroupDescription> sortableGroupDescriptions, GPUDrivenBRGMaterialMergeMode materialMergeMode, JobHandle inputDeps)
	{
		if (sortableGroupDescriptions.Length >= 100)
		{
			return GroupSortingAlgorithmSelector.Big(sortableGroupDescriptions, materialMergeMode, inputDeps);
		}
		return GroupSortingAlgorithmSelector.Small(sortableGroupDescriptions, materialMergeMode, inputDeps);
	}
}
