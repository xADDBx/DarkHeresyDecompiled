using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Core.Collections;
using Owlcat.Runtime.Core.Math;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Batching;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Profiling.Memory;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Scenes;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.ShaderLibrary.Visual.Debug;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public class GPUDrivenRendererGroupPool : IDisposable, IGPUDrivenMemoryProfilingSource
{
	public enum DirtyState
	{
		UpToDate,
		DirtyOnCPU,
		DirtyOnGPU
	}

	[Flags]
	public enum RendererGroupFlags
	{
		None = 0,
		StaticShadowCater = 1,
		ReceiveShadows = 2,
		FlipWinding = 4,
		Disabled = 8,
		UseLightMaps = 0x10,
		UseLightProbes = 0x20,
		AffectsLightMaps = 0x40,
		LODCrossFade = 0x100
	}

	[Flags]
	public enum RendererUpdateFlags
	{
		Nothing = 0,
		ForcedUpdate = 1,
		RendererData = 2,
		MaterialData = 4,
		CustomPerInstanceData = 8
	}

	public enum ViewType
	{
		Camera,
		DepthOnly,
		CameraMotionVectors,
		Shadows,
		ShadowsCullLightmapped
	}

	[BurstCompile]
	private struct SortGroupInstanceIndicesJob : IJobFor
	{
		private struct Comparer : IComparer<int>
		{
			public SortGroupInstanceIndicesJob Job;

			public int Compare(int x, int y)
			{
				GPUDrivenBatchRendererGroup.InstanceMetadata metadata = Job.InstanceMetadata[x];
				GPUDrivenBatchRendererGroup.InstanceMetadata metadata2 = Job.InstanceMetadata[y];
				return CompareInstances(in metadata, in metadata2);
			}
		}

		[ReadOnly]
		public NativeArray<GPUDrivenBatchRendererGroup.InstanceMetadata>.ReadOnly InstanceMetadata;

		[ReadOnly]
		public NativeArray<GPUDrivenIndexAllocator.IndexAllocation> Groups;

		[NativeDisableParallelForRestriction]
		public NativeArray<UnsafeList<int>> GroupIndices;

		public void Execute(int index)
		{
			GPUDrivenIndexAllocator.IndexAllocation indexAllocation = Groups[index];
			GroupIndices[indexAllocation.Index].Sort(new Comparer
			{
				Job = this
			});
		}
	}

	public struct PendingGroupRebuild : IDisposable
	{
		public bool InProgress;

		public JobHandle? SliceBuildingJobHandle;

		public NativeArray<int> TempSliceCount;

		public NativeArray<int> TempIndexCount;

		public JobHandle? GPUCullingJobsJobHandle;

		public NativeArray<int> TempGPUCullingJobsCount;

		public void Dispose()
		{
			if (SliceBuildingJobHandle.HasValue)
			{
				SliceBuildingJobHandle = null;
			}
			if (TempSliceCount.IsCreated)
			{
				TempSliceCount.Dispose();
			}
			if (TempIndexCount.IsCreated)
			{
				TempIndexCount.Dispose();
			}
			if (TempGPUCullingJobsCount.IsCreated)
			{
				TempGPUCullingJobsCount.Dispose();
			}
			if (GPUCullingJobsJobHandle.HasValue)
			{
				GPUCullingJobsJobHandle = null;
			}
		}
	}

	public struct ViewTypeInfo
	{
		public PendingGroupRebuild PendingRebuild;

		public ViewType ViewType;

		public DirtyState GroupSlicesDirtyState;

		public DirtyState GPUCullingJobsDirtyState;

		public int RendererGroupSlicesOffset;

		public int RendererGroupSlicesCount;

		public int RendererGroupSlicesMaxCount;

		public int GroupInfoOffset;

		public int GroupInfoCount;

		public int GroupInfoMaxCount;

		public int GPUCullingJobsOffset;

		public int GPUCullingJobsCount;

		public int GPUCullingJobsMaxCount;

		public int IndicesOffset;

		public int IndicesCount;

		public int IndicesMaxCount;
	}

	[BurstCompile]
	private struct FillGroupInfoJob : IJobFor
	{
		[ReadOnly]
		public NativeSlice<RendererGroupSlice> RendererGroupSlices;

		[ReadOnly]
		public NativeArray<RendererGroup> RendererGroups;

		[ReadOnly]
		public NativeArray<GPUDrivenResourceRegistry.UnmanagedMeshInfo>.ReadOnly Meshes;

		[WriteOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<GPUDrivenComputeShaders.GroupInfo> GroupInfos;

		public void Execute(int index)
		{
			RendererGroupSlice rendererGroupSlice = RendererGroupSlices[index];
			RendererGroup rendererGroup = RendererGroups[rendererGroupSlice.GroupIndexAllocation.Index];
			GPUDrivenResourceRegistry.UnmanagedMeshInfo unmanagedMeshInfo = Meshes[rendererGroup.Key.MeshAllocation.Index];
			ref GPUDrivenComputeShaders.GroupInfo reference = ref UnsafeCollectionExtensions.ElementAsRefUnchecked(in GroupInfos, index);
			reference.MeshIndexStart = unmanagedMeshInfo.IndexStart;
			reference.MeshIndexCount = unmanagedMeshInfo.IndexCount;
			reference.PersistentInstanceIndexStart = (uint)rendererGroupSlice.PersistentIndexOffset;
			reference.VisibleInstanceIndexStart = (uint)rendererGroupSlice.VisibleIndexOffset;
			reference.InstanceIndexCount = (uint)rendererGroupSlice.IndexCount;
		}
	}

	[BurstCompile]
	private struct CreateGPUCullingJobsJob : IJob
	{
		public int WrittenGroupOffset;

		[ReadOnly]
		public NativeSlice<RendererGroupSlice> RendererGroupSlices;

		[ReadOnly]
		public NativeArray<RendererGroup> RendererGroups;

		[ReadOnly]
		public NativeArray<GPUDrivenResourceRegistry.UnmanagedMaterialInfo>.ReadOnly Materials;

		[WriteOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<GPUDrivenComputeShaders.GPUCullingJob> GPUCullingJobs;

		[NativeDisableUnsafePtrRestriction]
		public unsafe int* CullingJobsCountPtr;

		public unsafe void Execute()
		{
			*CullingJobsCountPtr = 0;
			int num = 0;
			for (int i = 0; i < RendererGroupSlices.Length; i++)
			{
				RendererGroupSlice rendererGroupSlice = RendererGroupSlices[i];
				int num2;
				for (int j = 0; j < rendererGroupSlice.IndexCount; j += num2)
				{
					num2 = math.min(rendererGroupSlice.IndexCount - j, 32);
					ref GPUDrivenComputeShaders.GPUCullingJob reference = ref UnsafeCollectionExtensions.ElementAsRef(in GPUCullingJobs, num);
					reference.GroupIndex = (uint)(i + WrittenGroupOffset);
					reference.Offset = (uint)j;
					reference.Count = (uint)num2;
					reference.Flags = CollectJobFlags(rendererGroupSlice.GroupIndexAllocation);
					num++;
				}
			}
			*CullingJobsCountPtr = num;
		}

		private GPUDrivenComputeShaders.GPUCullingJobFlags CollectJobFlags(GPUDrivenIndexAllocator.IndexAllocation groupIndexAllocation)
		{
			GPUDrivenComputeShaders.GPUCullingJobFlags gPUCullingJobFlags = GPUDrivenComputeShaders.GPUCullingJobFlags.None;
			RendererGroup rendererGroup = RendererGroups[groupIndexAllocation.Index];
			if ((Materials[rendererGroup.Key.MaterialAllocation.Index].Flags & GPUDrivenResourceRegistry.MaterialFlags.Transparent) != 0)
			{
				gPUCullingJobFlags |= GPUDrivenComputeShaders.GPUCullingJobFlags.TransparentMaterial;
			}
			return gPUCullingJobFlags;
		}
	}

	[BurstCompile]
	private struct RendererGroupMergeabilityCheckJob : IJobParallelForBatch
	{
		public const int kBatchSize = 32;

		[ReadOnly]
		public NativeArray<GPUDrivenSortableGroupDescriptionManager.SortableGroupDescription> SortableGroupDescriptions;

		[WriteOnly]
		[NativeDisableParallelForRestriction]
		public NativeArray<int> MergeabilityMask;

		public void Execute(int startIndex, int count)
		{
			int num = 0;
			for (int i = 0; i < count; i++)
			{
				int num2 = startIndex + i;
				if (num2 <= 0)
				{
					continue;
				}
				GPUDrivenSortableGroupDescriptionManager.SortableGroupDescription sortableGroupDescription = SortableGroupDescriptions[num2];
				if (sortableGroupDescription.AllowMerging())
				{
					GPUDrivenSortableGroupDescriptionManager.SortableGroupDescription other = SortableGroupDescriptions[num2 - 1];
					if (sortableGroupDescription.CompareTo(other) == 0)
					{
						num |= 1 << i;
					}
				}
			}
			int index = startIndex / 32;
			MergeabilityMask[index] = num;
		}
	}

	[BurstCompile]
	private struct PopulateRendererGroupSlicesJob : IJobParallelForBatch
	{
		public const int kBatchSize = 256;

		public int TotalWrittenIndexOffset;

		public bool AllowMerging;

		public bool DrawOneByOne;

		public bool DisableDecalSorting;

		public bool DisableDecalBatching;

		[ReadOnly]
		public NativeArray<GPUDrivenBatchRendererGroup.InstanceMetadata>.ReadOnly InstanceMetadata;

		[ReadOnly]
		public NativeArray<RendererGroup> RendererGroups;

		[ReadOnly]
		public NativeArray<UnsafeList<int>> RendererGroupIndices;

		[ReadOnly]
		public NativeArray<GPUDrivenSortableGroupDescriptionManager.SortableGroupDescription> SortableGroupDescriptions;

		[ReadOnly]
		public NativeArray<int> GroupMergeabilityMask;

		[ReadOnly]
		public NativeArray<GPUDrivenResourceRegistry.UnmanagedMaterialInfo>.ReadOnly Materials;

		[WriteOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<int> AllIndices;

		[WriteOnly]
		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<RendererGroupSlice> RendererGroupSlices;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 IndicesCounter;

		[NativeDisableUnsafePtrRestriction]
		public UnsafeAtomicCounter32 RendererGroupSlicesCounter;

		public unsafe void Execute(int startIndex, int count)
		{
			int2 @int = default(int2);
			NativeList<RendererGroupSlice> nativeArray = new NativeList<RendererGroupSlice>(count, Allocator.Temp);
			NativeList<int> nativeList = new NativeList<int>(8 * count, Allocator.Temp);
			for (int i = 0; i < count; i++)
			{
				int num = startIndex + i;
				GPUDrivenSortableGroupDescriptionManager.SortableGroupDescription sortableGroupDescription = SortableGroupDescriptions[num];
				GPUDrivenIndexAllocator.IndexAllocation indexAllocation = sortableGroupDescription.IndexAllocation;
				RendererGroup rendererGroup = RendererGroups[indexAllocation.Index];
				UnsafeList<int> range = RendererGroupIndices[indexAllocation.Index];
				int length = range.Length;
				int length2 = nativeList.Length;
				nativeList.AddRange(range);
				int length3 = nativeArray.Length;
				bool allowMerging = AllowMerging && sortableGroupDescription.AllowMerging();
				GPUDrivenResourceRegistry.UnmanagedMaterialInfo materialInfo = Materials[rendererGroup.Key.MaterialAllocation.Index];
				int index = num / 32;
				int num2 = num % 32;
				int num3 = GroupMergeabilityMask[index];
				if (i > 0 && (num3 & (1 << num2)) != 0)
				{
					ref RendererGroupSlice reference = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, length3 - 1);
					if (reference.IsMergeable)
					{
						reference.IndexCount += length;
					}
					else
					{
						PopulateGroupSlices(in materialInfo, in rendererGroup, nativeArray, length2, allowMerging, num);
					}
				}
				else
				{
					PopulateGroupSlices(in materialInfo, in rendererGroup, nativeArray, length2, allowMerging, num);
				}
				if (DisableDecalSorting || !materialInfo.GPUDrivenMaterialUniquenessKey.RequiresCustomSorting())
				{
					continue;
				}
				if (@int.Equals(default(int2)))
				{
					@int = new int2(length3, nativeArray.Length);
				}
				else
				{
					@int.y = nativeArray.Length;
				}
				if (num < startIndex + count - 1)
				{
					GPUDrivenSortableGroupDescriptionManager.SortableGroupDescription sortableGroupDescription2 = SortableGroupDescriptions[num + 1];
					if (sortableGroupDescription2.GPUDrivenMaterialUniquenessKey.RequiresCustomSorting())
					{
						continue;
					}
				}
				GroupSliceCustomSortingJob groupSliceCustomSortingJob = default(GroupSliceCustomSortingJob);
				groupSliceCustomSortingJob.Indices = nativeList.AsArray();
				groupSliceCustomSortingJob.InstanceMetadata = InstanceMetadata;
				groupSliceCustomSortingJob.RendererGroupSlices = nativeArray.AsArray();
				groupSliceCustomSortingJob.GroupSliceRange = @int;
				groupSliceCustomSortingJob.Execute();
				if (!DrawOneByOne && !DisableDecalBatching)
				{
					MergeCustomSortingGroupSlices(nativeArray, @int);
				}
				@int = default(int2);
			}
			int num4 = IndicesCounter.Add(nativeList.Length);
			if (num4 + nativeList.Length > AllIndices.Length)
			{
				Debug.LogError("While building indices, could not fit all groups into the buffer.");
				return;
			}
			UnsafeUtility.MemCpy((byte*)AllIndices.GetUnsafePtr() + (nint)num4 * (nint)4, nativeList.GetUnsafePtr(), nativeList.Length * 4);
			int num5 = RendererGroupSlicesCounter.Add(nativeArray.Length);
			UnsafeUtility.MemCpy((byte*)RendererGroupSlices.GetUnsafePtr() + (nint)num5 * (nint)sizeof(RendererGroupSlice), nativeArray.GetUnsafePtr(), nativeArray.Length * UnsafeUtility.SizeOf<RendererGroupSlice>());
			for (int j = 0; j < nativeArray.Length; j++)
			{
				ref RendererGroupSlice reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in RendererGroupSlices, num5 + j);
				reference2.VisibleIndexOffset += num4;
				reference2.PersistentIndexOffset += num4 + TotalWrittenIndexOffset;
			}
		}

		private void PopulateGroupSlices(in GPUDrivenResourceRegistry.UnmanagedMaterialInfo materialInfo, in RendererGroup rendererGroup, NativeList<RendererGroupSlice> rendererGroupSlices, int groupIndexOffset, bool allowMerging, int sortableGroupDescriptionIndex)
		{
			int length = RendererGroupIndices[rendererGroup.IndexAllocation.Index].Length;
			bool flag = (materialInfo.Flags & GPUDrivenResourceRegistry.MaterialFlags.Transparent) != 0;
			bool flag2 = materialInfo.GPUDrivenMaterialUniquenessKey.RequiresCustomSorting();
			if (DrawOneByOne || flag || flag2)
			{
				for (int i = 0; i < length; i++)
				{
					RendererGroupSlice value = new RendererGroupSlice
					{
						GroupIndexAllocation = rendererGroup.IndexAllocation,
						PersistentIndexOffset = groupIndexOffset + i,
						VisibleIndexOffset = groupIndexOffset + i,
						IndexCount = 1,
						IsTransparent = flag,
						IsMergeable = false,
						SortableGroupDescriptionIndex = sortableGroupDescriptionIndex
					};
					rendererGroupSlices.Add(in value);
				}
			}
			else
			{
				RendererGroupSlice value = new RendererGroupSlice
				{
					GroupIndexAllocation = rendererGroup.IndexAllocation,
					PersistentIndexOffset = groupIndexOffset,
					VisibleIndexOffset = groupIndexOffset,
					IndexCount = length,
					IsTransparent = flag,
					IsMergeable = allowMerging,
					SortableGroupDescriptionIndex = sortableGroupDescriptionIndex
				};
				rendererGroupSlices.Add(in value);
			}
		}

		private void MergeCustomSortingGroupSlices(NativeList<RendererGroupSlice> rendererGroupSlices, int2 sliceRange)
		{
			int num = sliceRange.y - sliceRange.x;
			NativeList<RendererGroupSlice> nativeArray = new NativeList<RendererGroupSlice>(num, Allocator.Temp);
			for (int i = sliceRange.x; i < sliceRange.y; i++)
			{
				ref RendererGroupSlice reference = ref UnsafeCollectionExtensions.ElementAsRef(in rendererGroupSlices, i);
				if (nativeArray.Length == 0)
				{
					nativeArray.Add(in reference);
					continue;
				}
				ref RendererGroupSlice reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, nativeArray.Length - 1);
				if (CanMergeCustomSortedGroupSlices(ref reference2, ref reference))
				{
					reference2.IndexCount += reference.IndexCount;
				}
				else
				{
					nativeArray.Add(in reference);
				}
			}
			if (nativeArray.Length != num)
			{
				rendererGroupSlices.Length -= num;
				rendererGroupSlices.AddRange(nativeArray.AsArray());
			}
		}

		private bool CanMergeCustomSortedGroupSlices(ref RendererGroupSlice destinationGroup, ref RendererGroupSlice sourceGroup)
		{
			if (destinationGroup.PersistentIndexOffset + destinationGroup.IndexCount != sourceGroup.PersistentIndexOffset || destinationGroup.VisibleIndexOffset + destinationGroup.IndexCount != sourceGroup.VisibleIndexOffset)
			{
				return false;
			}
			if (!destinationGroup.GroupIndexAllocation.Equals(sourceGroup.GroupIndexAllocation) && SortableGroupDescriptions[destinationGroup.SortableGroupDescriptionIndex].CompareTo(SortableGroupDescriptions[sourceGroup.SortableGroupDescriptionIndex]) != 0)
			{
				return false;
			}
			return true;
		}
	}

	[BurstCompile]
	private struct GroupSliceCustomSortingJob : IJob
	{
		private struct Comparer : IComparer<RendererGroupSlice>
		{
			public GroupSliceCustomSortingJob Job;

			public int Compare(RendererGroupSlice slice1, RendererGroupSlice slice2)
			{
				GPUDrivenBatchRendererGroup.InstanceMetadata metadata = GetInstanceMetadata(in slice1);
				GPUDrivenBatchRendererGroup.InstanceMetadata metadata2 = GetInstanceMetadata(in slice2);
				return CompareInstances(in metadata, in metadata2);
			}

			private GPUDrivenBatchRendererGroup.InstanceMetadata GetInstanceMetadata(in RendererGroupSlice slice)
			{
				int index = Job.Indices[slice.PersistentIndexOffset];
				return Job.InstanceMetadata[index];
			}
		}

		public int2 GroupSliceRange;

		[ReadOnly]
		public NativeSlice<int> Indices;

		[ReadOnly]
		public NativeArray<GPUDrivenBatchRendererGroup.InstanceMetadata>.ReadOnly InstanceMetadata;

		[NativeDisableContainerSafetyRestriction]
		public NativeSlice<RendererGroupSlice> RendererGroupSlices;

		public void Execute()
		{
			if (GroupSliceRange.x < GroupSliceRange.y - 1)
			{
				Comparer comparer = default(Comparer);
				comparer.Job = this;
				Comparer comp = comparer;
				RendererGroupSlices.Slice(GroupSliceRange.x, GroupSliceRange.y - GroupSliceRange.x).Sort(comp);
			}
		}
	}

	private static class Profiling
	{
		public static readonly ProfilingSampler PostRender = new ProfilingSampler("GPUDrivenRendererGroupPool PostRender");

		public static readonly ProfilingSampler StartSortableGroupDescriptionJobs = new ProfilingSampler("StartSortableGroupDescriptionJobs");

		public static readonly ProfilingSampler ScheduleSortGroupsAndPopulateSlices = new ProfilingSampler("ScheduleSortGroupsAndPopulateSlices");

		public static readonly ProfilingSampler SortGroupInstanceIndices = new ProfilingSampler("SortGroupInstanceIndices");

		public static readonly ProfilingSampler BeginRebuildingGroupInfo = new ProfilingSampler("BeginRebuildingGroupInfo");

		public static readonly ProfilingSampler CompleteRebuildingGroupSlices = new ProfilingSampler("CompleteRebuildingGroupSlices");

		public static readonly ProfilingSampler CompleteRebuildingGroupInfo = new ProfilingSampler("CompleteRebuildingGroupInfo");

		public static readonly ProfilingSampler UploadGroups = new ProfilingSampler("UploadGroups");

		public static readonly ProfilingSampler ScheduleFillGroupInfoAndGPUCullingJobs = new ProfilingSampler("ScheduleFillGroupInfoAndGPUCullingJobs");
	}

	public struct RendererGroupSlice
	{
		public GPUDrivenIndexAllocator.IndexAllocation GroupIndexAllocation;

		public int PersistentIndexOffset;

		public int VisibleIndexOffset;

		public int IndexCount;

		public bool IsTransparent;

		public bool IsMergeable;

		public int SortableGroupDescriptionIndex;
	}

	public struct RendererGroupKey : IEquatable<RendererGroupKey>
	{
		public GPUDrivenIndexAllocator.IndexAllocation MaterialAllocation;

		public GPUDrivenIndexAllocator.IndexAllocation MeshAllocation;

		public RendererSettings RendererSettings;

		public bool Equals(RendererGroupKey other)
		{
			if (MaterialAllocation.Equals(other.MaterialAllocation) && MeshAllocation.Equals(other.MeshAllocation))
			{
				return RendererSettings.Equals(other.RendererSettings);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is RendererGroupKey other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(MaterialAllocation.GetHashCode(), MeshAllocation.GetHashCode(), RendererSettings);
		}

		public override string ToString()
		{
			return $"Material={MaterialAllocation};Mesh={MeshAllocation};Settings={RendererSettings}";
		}
	}

	public struct RendererSettingsCreationInfo
	{
		public GPUDrivenBRGSettings BRGSettings;

		public bool LODCrossFadeEnabled;
	}

	public struct RendererSettings : IEquatable<RendererSettings>, IComparable<RendererSettings>
	{
		public RendererGroupFlags Flags;

		public ShadowCastingMode ShadowCastingMode;

		public MotionVectorGenerationMode MotionVectorGenerationMode;

		public uint RenderingLayerMask;

		public byte Layer;

		public GPUDrivenSceneHandle Scene;

		public static RendererSettings From(in GPUDrivenBatchRendererGroup.RendererDesc rendererDesc, in GPUDrivenBatchRendererGroup.RendererParams rendererParams, RendererSettingsCreationInfo settings)
		{
			RendererSettings result = default(RendererSettings);
			result.Flags = RendererGroupFlags.None;
			GPUDrivenRendererParamsExtensions.ExtractedRendererSettings rendererSettings = rendererDesc.GetRendererSettings(in rendererParams);
			if (!rendererSettings.Enabled)
			{
				RendererSettings rendererSettings2 = default(RendererSettings);
				rendererSettings2.Flags = RendererGroupFlags.Disabled;
				result = rendererSettings2;
			}
			else
			{
				result.MotionVectorGenerationMode = rendererSettings.General.MotionVectorGenerationMode;
				result.ShadowCastingMode = rendererSettings.General.ShadowCastingMode;
				if (rendererSettings.General.StaticShadowCaster)
				{
					result.Flags |= RendererGroupFlags.StaticShadowCater;
				}
				if (rendererSettings.General.ReceiveShadows)
				{
					result.Flags |= RendererGroupFlags.ReceiveShadows;
				}
				if (ShouldFlipWinding(rendererSettings.Scale))
				{
					result.Flags |= RendererGroupFlags.FlipWinding;
				}
				if (rendererSettings.General.LightmapIndex != -1)
				{
					result.Flags |= RendererGroupFlags.AffectsLightMaps;
				}
				if (settings.LODCrossFadeEnabled && rendererDesc.MeshRenderer != null)
				{
					LODGroup lODGroup = rendererDesc.MeshRenderer.LODGroup;
					if (lODGroup != null && lODGroup.enabled && lODGroup.fadeMode == LODFadeMode.CrossFade)
					{
						result.Flags |= RendererGroupFlags.LODCrossFade;
					}
				}
				result.RenderingLayerMask = rendererSettings.General.RenderingLayerMask;
				result.Layer = (byte)rendererSettings.General.Layer;
				Scene scene = rendererDesc.GetScene(in rendererParams);
				result.Scene = GPUDrivenSceneHandle.FromScene(in scene);
			}
			int lightmapIndex = rendererSettings.General.LightmapIndex;
			if (lightmapIndex >= 0 && lightmapIndex <= 65533)
			{
				result.Flags |= RendererGroupFlags.UseLightMaps;
			}
			else if (rendererSettings.General.LightProbeUsage == LightProbeUsage.BlendProbes && !ProbeReferenceVolume.instance.isInitialized)
			{
				result.Flags |= RendererGroupFlags.UseLightProbes;
			}
			return result;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ShouldFlipWinding(Vector3 scale)
		{
			return scale.x * scale.y * scale.z < 0f;
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static bool ShouldFlipWinding(float4x4 localToWorldMatrix)
		{
			return math.determinant((float3x3)localToWorldMatrix) < 0f;
		}

		public bool Equals(RendererSettings other)
		{
			if (Flags == other.Flags && ShadowCastingMode == other.ShadowCastingMode && MotionVectorGenerationMode == other.MotionVectorGenerationMode && RenderingLayerMask == other.RenderingLayerMask && Layer == other.Layer)
			{
				return Scene.Equals(other.Scene);
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is RendererSettings other)
			{
				return Equals(other);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int)Flags, (int)ShadowCastingMode, (int)MotionVectorGenerationMode, (int)RenderingLayerMask, (int)Layer, Scene);
		}

		public int CompareTo(RendererSettings other)
		{
			int flags = (int)Flags;
			int num = flags.CompareTo((int)other.Flags);
			if (num != 0)
			{
				return num;
			}
			flags = (int)ShadowCastingMode;
			int num2 = flags.CompareTo((int)other.ShadowCastingMode);
			if (num2 != 0)
			{
				return num2;
			}
			flags = (int)MotionVectorGenerationMode;
			int num3 = flags.CompareTo((int)other.MotionVectorGenerationMode);
			if (num3 != 0)
			{
				return num3;
			}
			int num4 = RenderingLayerMask.CompareTo(other.RenderingLayerMask);
			if (num4 != 0)
			{
				return num4;
			}
			int num5 = Layer.CompareTo(other.Layer);
			if (num5 != 0)
			{
				return num5;
			}
			return Scene.CompareTo(other.Scene);
		}
	}

	public struct RendererGroup
	{
		public RendererGroupKey Key;

		public GPUDrivenIndexAllocator.IndexAllocation IndexAllocation;

		public bool PendingSorting;
	}

	public const int kInvalidRendererGroupIndex = -1;

	private readonly GPUDrivenBatchRendererGroup m_BRG;

	private readonly GPUDrivenResourceRegistry m_ResourceRegistry;

	private readonly GPUDrivenSortableGroupDescriptionManager m_SortableGroupDescriptionManager;

	private readonly WaaaghDebugData m_WaaaghDebugData;

	private NativeArray<int> m_AllIndices;

	private NativeList<GPUDrivenIndexAllocator.IndexAllocation> m_EmptyGroupIndices;

	private NativeArray<GPUDrivenComputeShaders.GPUCullingJob> m_GPUCullingJobs;

	private ResizableGraphicsBuffer m_GPUCullingJobsBuffer;

	private ResizableGraphicsBuffer m_GroupInfoBuffer;

	private NativeArray<GPUDrivenComputeShaders.GroupInfo> m_GroupInfos;

	private ResizableGraphicsBuffer m_PersistentIndicesBuffer;

	private GPUDrivenIndexAllocator m_RendererGroupIndexAllocator;

	private NativeArray<UnsafeList<int>> m_RendererGroupIndices;

	private NativeHashMap<RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation> m_RendererGroupKeyToIndex;

	private NativeArray<RendererGroup> m_RendererGroups;

	private NativeArray<RendererGroupSlice> m_RendererGroupSlices;

	private NativeList<GPUDrivenIndexAllocator.IndexAllocation> m_RendererGroupsToSort;

	public ViewTypeInfo[] ViewTypeInfos
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get;
	}

	public ref readonly ResizableGraphicsBuffer PersistentIndicesBuffer => ref m_PersistentIndicesBuffer;

	public ref readonly ResizableGraphicsBuffer GroupInfoBuffer => ref m_GroupInfoBuffer;

	public ref readonly ResizableGraphicsBuffer GPUCullingJobsBuffer => ref m_GPUCullingJobsBuffer;

	public NativeHashMap<RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation>.ReadOnly ActiveRendererGroups => m_RendererGroupKeyToIndex.AsReadOnly();

	public GPUDrivenRendererGroupPool(GPUDrivenBatchRendererGroup brg, GPUDrivenResourceRegistry resourceRegistry, [CanBeNull] WaaaghDebugData waaaghDebugData)
	{
		m_WaaaghDebugData = waaaghDebugData;
		ViewTypeInfos = ((ViewType[])Enum.GetValues(typeof(ViewType))).Select(delegate(ViewType vt)
		{
			ViewTypeInfo result = default(ViewTypeInfo);
			result.ViewType = vt;
			return result;
		}).ToArray();
		m_BRG = brg;
		m_ResourceRegistry = resourceRegistry;
		GPUDrivenBRGSettings settings = brg.Settings;
		m_RendererGroupKeyToIndex = new NativeHashMap<RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation>(settings.InitialRendererGroupsCapacity, Allocator.Persistent);
		ResizeOrCreateRendererGroupBuffers(settings.InitialRendererGroupsCapacity);
		ResizeInstanceCount(settings.InitialInstanceCapacity);
		m_EmptyGroupIndices = new NativeList<GPUDrivenIndexAllocator.IndexAllocation>(16, Allocator.Persistent);
		m_RendererGroupsToSort = new NativeList<GPUDrivenIndexAllocator.IndexAllocation>(settings.InitialRendererGroupsCapacity, Allocator.Persistent);
		m_SortableGroupDescriptionManager = new GPUDrivenSortableGroupDescriptionManager(this, m_ResourceRegistry);
	}

	public void Dispose()
	{
		for (int i = 0; i < ViewTypeInfos.Length; i++)
		{
			CompleteAllRebuilding(ViewTypeInfos[i].ViewType);
		}
		if (m_RendererGroupIndices.IsCreated)
		{
			foreach (UnsafeList<int> rendererGroupIndex in m_RendererGroupIndices)
			{
				if (rendererGroupIndex.IsCreated)
				{
					rendererGroupIndex.Dispose();
				}
			}
			m_RendererGroupIndices.Dispose();
			m_RendererGroupIndices = default(NativeArray<UnsafeList<int>>);
		}
		if (m_RendererGroups.IsCreated)
		{
			m_RendererGroups.Dispose();
			m_RendererGroups = default(NativeArray<RendererGroup>);
		}
		m_RendererGroupIndexAllocator.Dispose();
		if (m_RendererGroupKeyToIndex.IsCreated)
		{
			m_RendererGroupKeyToIndex.Dispose();
			m_RendererGroupKeyToIndex = default(NativeHashMap<RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation>);
		}
		if (m_AllIndices.IsCreated)
		{
			m_AllIndices.Dispose();
			m_AllIndices = default(NativeArray<int>);
		}
		m_PersistentIndicesBuffer.Dispose();
		if (m_GroupInfos.IsCreated)
		{
			m_GroupInfos.Dispose();
			m_GroupInfos = default(NativeArray<GPUDrivenComputeShaders.GroupInfo>);
		}
		if (m_GPUCullingJobs.IsCreated)
		{
			m_GPUCullingJobs.Dispose();
			m_GPUCullingJobs = default(NativeArray<GPUDrivenComputeShaders.GPUCullingJob>);
		}
		m_GroupInfoBuffer.Dispose();
		m_GPUCullingJobsBuffer.Dispose();
		if (m_RendererGroupSlices.IsCreated)
		{
			m_RendererGroupSlices.Dispose();
			m_RendererGroupSlices = default(NativeArray<RendererGroupSlice>);
		}
		if (m_EmptyGroupIndices.IsCreated)
		{
			m_EmptyGroupIndices.Dispose();
			m_EmptyGroupIndices = default(NativeList<GPUDrivenIndexAllocator.IndexAllocation>);
		}
		if (m_RendererGroupsToSort.IsCreated)
		{
			m_RendererGroupsToSort.Dispose();
			m_RendererGroupsToSort = default(NativeList<GPUDrivenIndexAllocator.IndexAllocation>);
		}
	}

	public void FillMemoryCounters(Counters.CounterCollection counters)
	{
		counters.CollectBufferSize(counters.InstanceDataCPU, m_AllIndices);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_EmptyGroupIndices);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_RendererGroupIndices);
		foreach (UnsafeList<int> rendererGroupIndex in m_RendererGroupIndices)
		{
			counters.CollectBufferSize(counters.InstanceDataCPU, rendererGroupIndex);
		}
		counters.CollectBufferSize(counters.InstanceDataCPU, m_RendererGroupKeyToIndex);
		counters.CollectBufferSize(counters.InstanceDataCPU, m_RendererGroups);
		counters.CollectBufferSize(counters.InstanceDataGPU, m_PersistentIndicesBuffer);
		counters.CollectBufferSize(counters.CullingBatchingCPU, m_GPUCullingJobs);
		counters.CollectBufferSize(counters.CullingBatchingGPU, m_GPUCullingJobsBuffer);
		counters.CollectBufferSize(counters.CullingBatchingCPU, m_GroupInfos);
		counters.CollectBufferSize(counters.CullingBatchingGPU, m_GroupInfoBuffer);
		counters.CollectBufferSize(counters.CullingBatchingCPU, m_RendererGroupSlices);
		counters.CollectBufferSize(counters.CullingBatchingCPU, m_RendererGroupsToSort);
		m_RendererGroupIndexAllocator.FillMemoryCounter(counters, counters.InstanceDataCPU);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal NativeArray<UnsafeList<int>> GetRendererGroupIndices()
	{
		return m_RendererGroupIndices;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal NativeHashMap<RendererGroupKey, GPUDrivenIndexAllocator.IndexAllocation> GetRendererGroupKeyToIndex()
	{
		return m_RendererGroupKeyToIndex;
	}

	private void ResizeOrCreateRendererGroupBuffers(int count)
	{
		if (m_RendererGroupIndexAllocator == null)
		{
			m_RendererGroupIndexAllocator = new GPUDrivenIndexAllocator(count);
		}
		else
		{
			m_RendererGroupIndexAllocator.ForceGrow(count);
		}
		count = m_RendererGroupIndexAllocator.Capacity;
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_RendererGroups, count, Allocator.Persistent);
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_RendererGroupIndices, count, Allocator.Persistent);
	}

	public void ResizeInstanceCount(int instanceCount)
	{
		int size = instanceCount * ViewTypeInfos.Length;
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_AllIndices, size, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
		m_PersistentIndicesBuffer.CreateOrResize(GraphicsBuffer.Target.Raw, m_AllIndices.Length, UnsafeUtility.SizeOf<uint>());
		int size2 = instanceCount * ViewTypeInfos.Length;
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_RendererGroupSlices, size2, Allocator.Persistent);
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_GroupInfos, m_RendererGroupSlices.Length, Allocator.Persistent);
		int size3 = instanceCount * ViewTypeInfos.Length;
		GPUDrivenRenderingUtils.CreateOrResizeNativeArray(ref m_GPUCullingJobs, size3, Allocator.Persistent);
		m_GroupInfoBuffer.CreateOrResize(GraphicsBuffer.Target.Structured, m_GroupInfos.Length, UnsafeUtility.SizeOf<GPUDrivenComputeShaders.GroupInfo>());
		m_GPUCullingJobsBuffer.CreateOrResize(GraphicsBuffer.Target.Structured, m_GPUCullingJobs.Length, UnsafeUtility.SizeOf<GPUDrivenComputeShaders.GPUCullingJob>());
		for (int i = 0; i < ViewTypeInfos.Length; i++)
		{
			ref ViewTypeInfo reference = ref ViewTypeInfos[i];
			reference.GroupInfoOffset = instanceCount * i;
			reference.GroupInfoMaxCount = instanceCount;
			reference.RendererGroupSlicesOffset = instanceCount * i;
			reference.RendererGroupSlicesMaxCount = instanceCount;
			reference.GPUCullingJobsOffset = instanceCount * i;
			reference.GPUCullingJobsMaxCount = instanceCount;
			reference.IndicesOffset = instanceCount * i;
			reference.IndicesMaxCount = instanceCount;
		}
		OnModifiedCPUData();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<RendererGroupSlice>.ReadOnly GetAllRendererGroupSlicesReadonly(ViewType viewType)
	{
		ref ViewTypeInfo reference = ref ViewTypeInfos[(int)viewType];
		return m_RendererGroupSlices.GetSubArray(reference.RendererGroupSlicesOffset, reference.RendererGroupSlicesCount).AsReadOnly();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<int>.ReadOnly GetAllIndicesReadonly(ViewType viewType)
	{
		ref ViewTypeInfo reference = ref ViewTypeInfos[(int)viewType];
		return m_AllIndices.GetSubArray(reference.IndicesOffset, reference.IndicesCount).AsReadOnly();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public NativeArray<RendererGroup>.ReadOnly GetInnerPool()
	{
		return m_RendererGroups.AsReadOnly();
	}

	public ref readonly RendererGroup Read(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref Get(indexAllocation.Index);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public ref RendererGroup Get(int index)
	{
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_RendererGroups, index);
	}

	public ref readonly UnsafeList<int> ReadIndices(GPUDrivenIndexAllocator.IndexAllocation indexAllocation)
	{
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_RendererGroupIndices, indexAllocation.Index);
	}

	public ref UnsafeList<int> GetIndices(int index)
	{
		return ref UnsafeCollectionExtensions.ElementAsRef(in m_RendererGroupIndices, index);
	}

	public void AddInstanceToGroup(ref RendererGroup rendererGroup, GPUDrivenIndexAllocator.IndexAllocation instanceIndex)
	{
		UnsafeCollectionExtensions.ElementAsRef(in m_RendererGroupIndices, rendererGroup.IndexAllocation.Index).Add(in instanceIndex.Index);
		ScheduleSorting(ref rendererGroup);
		OnModifiedCPUData();
	}

	public void RemoveInstanceFromGroup(ref RendererGroup rendererGroup, GPUDrivenIndexAllocator.IndexAllocation instanceIndex)
	{
		ref UnsafeList<int> reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_RendererGroupIndices, rendererGroup.IndexAllocation.Index);
		int index = reference.IndexOf(instanceIndex.Index);
		reference.RemoveAtSwapBack(index);
		ScheduleSorting(ref rendererGroup);
		OnModifiedCPUData();
		if (reference.Length == 0)
		{
			FreeGroup(ref rendererGroup);
		}
	}

	private void FreeGroup(ref RendererGroup rendererGroup)
	{
		m_RendererGroupKeyToIndex.TryGetValue(rendererGroup.Key, out var item);
		m_RendererGroupKeyToIndex.Remove(rendererGroup.Key);
		m_RendererGroupIndexAllocator.Free(item);
		int num = 0;
		while (num < m_RendererGroupsToSort.Length)
		{
			if (m_RendererGroupsToSort[num].Equals(rendererGroup.IndexAllocation))
			{
				m_RendererGroupsToSort.RemoveAtSwapBack(num);
			}
			else
			{
				num++;
			}
		}
		m_ResourceRegistry.GetUnmanagedMaterialInfo(in rendererGroup).ReferenceCount--;
		m_ResourceRegistry.TryFreeMaterial(rendererGroup.Key.MaterialAllocation);
		m_ResourceRegistry.GetUnmanagedMeshInfo(in rendererGroup).ReferenceCount--;
		m_ResourceRegistry.TryFreeMesh(rendererGroup.Key.MeshAllocation);
		ref UnsafeList<int> reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_RendererGroupIndices, rendererGroup.IndexAllocation.Index);
		reference.Dispose();
		reference = default(UnsafeList<int>);
		rendererGroup.Key = default(RendererGroupKey);
		rendererGroup.IndexAllocation = GPUDrivenIndexAllocator.IndexAllocation.Invalid;
	}

	public bool TryMigrateInstanceSimple(ref GPUDrivenBatchRendererGroup.InstanceData instanceData, in RendererGroupKey newGroupKey)
	{
		int orAllocate = GetOrAllocate(in newGroupKey);
		if (orAllocate == instanceData.RendererGroupIndex)
		{
			return false;
		}
		int rendererGroupIndex = instanceData.RendererGroupIndex;
		if (orAllocate != -1)
		{
			AddInstanceToGroup(ref Get(orAllocate), instanceData.IndexAllocation);
			instanceData.RendererGroupIndex = orAllocate;
		}
		RemoveInstanceFromGroup(ref Get(rendererGroupIndex), instanceData.IndexAllocation);
		return true;
	}

	public int GetOrAllocate(in RendererGroupKey key)
	{
		if (m_RendererGroupKeyToIndex.TryGetValue(key, out var item))
		{
			return item.Index;
		}
		GPUDrivenIndexAllocator.IndexAllocation indexAllocation = m_RendererGroupIndexAllocator.Allocate();
		if (indexAllocation.Index == -1)
		{
			ResizeOrCreateRendererGroupBuffers(m_RendererGroupIndexAllocator.Capacity * 2);
			indexAllocation = m_RendererGroupIndexAllocator.Allocate();
			if (indexAllocation.Index == -1)
			{
				LogFailedToAllocateError();
				return -1;
			}
		}
		m_RendererGroupKeyToIndex.Add(key, indexAllocation);
		m_RendererGroups[indexAllocation.Index] = new RendererGroup
		{
			Key = key,
			IndexAllocation = indexAllocation
		};
		m_RendererGroupIndices[indexAllocation.Index] = new UnsafeList<int>(16, Allocator.Persistent);
		m_ResourceRegistry.GetUnmanagedMaterialInfo(in key).ReferenceCount++;
		m_ResourceRegistry.GetUnmanagedMeshInfo(in key).ReferenceCount++;
		return indexAllocation.Index;
	}

	private static void LogFailedToAllocateError()
	{
		Debug.LogError("Failed to allocate a renderer group index. Out of memory.");
	}

	public void UploadGroupsToGPU(CommandBuffer cmd)
	{
		using (new ProfilingScope(Profiling.UploadGroups))
		{
			for (int i = 0; i < ViewTypeInfos.Length; i++)
			{
				ref ViewTypeInfo reference = ref ViewTypeInfos[i];
				CompleteAllRebuilding(reference.ViewType);
				if (reference.GroupSlicesDirtyState == DirtyState.DirtyOnGPU)
				{
					if (reference.IndicesCount > 0)
					{
						int indicesOffset = reference.IndicesOffset;
						cmd.SetBufferData(PersistentIndicesBuffer.InternalBuffer, m_AllIndices, indicesOffset, indicesOffset, reference.IndicesCount);
					}
					reference.GroupSlicesDirtyState = DirtyState.UpToDate;
				}
				if (reference.GPUCullingJobsDirtyState == DirtyState.DirtyOnGPU)
				{
					if (reference.GroupInfoCount > 0)
					{
						int groupInfoOffset = reference.GroupInfoOffset;
						cmd.SetBufferData(GroupInfoBuffer.InternalBuffer, m_GroupInfos, groupInfoOffset, groupInfoOffset, reference.GroupInfoCount);
					}
					if (reference.GPUCullingJobsCount > 0)
					{
						int gPUCullingJobsOffset = reference.GPUCullingJobsOffset;
						cmd.SetBufferData(GPUCullingJobsBuffer.InternalBuffer, m_GPUCullingJobs, gPUCullingJobsOffset, gPUCullingJobsOffset, reference.GPUCullingJobsCount);
					}
					reference.GPUCullingJobsDirtyState = DirtyState.UpToDate;
				}
			}
		}
	}

	private unsafe JobHandle ScheduleSortGroupsAndPopulateSlices(GPUDrivenSortableGroupDescriptionManager.JobInfo sortableGroupDescriptionJob, int* pSlicesCount, int* pIndexCount)
	{
		sortableGroupDescriptionJob.CollectJob.Complete();
		JobHandle dependsOn = default(JobHandle);
		NativeArray<GPUDrivenSortableGroupDescriptionManager.SortableGroupDescription> sortableGroupDescriptions = sortableGroupDescriptionJob.Descriptions.AsArray();
		GPUDrivenSortableGroupDescriptionManager.StripRedundantDataFromSortableGroupsJob jobData = default(GPUDrivenSortableGroupDescriptionManager.StripRedundantDataFromSortableGroupsJob);
		jobData.ViewType = sortableGroupDescriptionJob.ViewType;
		jobData.SortableGroupDescriptions = sortableGroupDescriptions;
		dependsOn = IJobParallelForExtensions.Schedule(jobData, sortableGroupDescriptions.Length, 32, dependsOn);
		GPUDrivenBRGMaterialMergeMode gPUDrivenBRGMaterialMergeMode;
		switch (sortableGroupDescriptionJob.ViewType)
		{
		case ViewType.Camera:
			gPUDrivenBRGMaterialMergeMode = m_BRG.Settings.MaterialMergeMode;
			break;
		case ViewType.DepthOnly:
		case ViewType.CameraMotionVectors:
		case ViewType.Shadows:
		case ViewType.ShadowsCullLightmapped:
			gPUDrivenBRGMaterialMergeMode = m_BRG.Settings.DepthOnlyMaterialMergeMode;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		GPUDrivenBRGMaterialMergeMode gPUDrivenBRGMaterialMergeMode2 = gPUDrivenBRGMaterialMergeMode;
		if (gPUDrivenBRGMaterialMergeMode2 != 0 && !(m_BRG.DebugData?.DisableGroupSorting ?? false))
		{
			dependsOn = GPUDrivenSortableGroupDescriptionManager.SortGroupDescriptions(sortableGroupDescriptions, gPUDrivenBRGMaterialMergeMode2, dependsOn);
		}
		bool flag = gPUDrivenBRGMaterialMergeMode2 != 0 && !(m_BRG.DebugData?.DisableGroupMerging ?? false);
		NativeArray<int> nativeArray = new NativeArray<int>(Alignment.AlignUp(sortableGroupDescriptions.Length, 32) / 32, Allocator.TempJob, (!flag) ? NativeArrayOptions.ClearMemory : NativeArrayOptions.UninitializedMemory);
		if (flag)
		{
			RendererGroupMergeabilityCheckJob jobData2 = default(RendererGroupMergeabilityCheckJob);
			jobData2.SortableGroupDescriptions = sortableGroupDescriptions;
			jobData2.MergeabilityMask = nativeArray;
			dependsOn = jobData2.ScheduleBatch(sortableGroupDescriptions.Length, 32, dependsOn);
		}
		NativeList<int2> nativeList = new NativeList<int2>(m_RendererGroupKeyToIndex.Count, Allocator.TempJob);
		ref ViewTypeInfo reference = ref ViewTypeInfos[(int)sortableGroupDescriptionJob.ViewType];
		NativeSlice<RendererGroupSlice> rendererGroupSlices = m_RendererGroupSlices.Slice(reference.RendererGroupSlicesOffset, reference.RendererGroupSlicesMaxCount);
		NativeSlice<int> allIndices = m_AllIndices.Slice(reference.IndicesOffset, reference.IndicesMaxCount);
		PopulateRendererGroupSlicesJob jobData3 = default(PopulateRendererGroupSlicesJob);
		jobData3.TotalWrittenIndexOffset = reference.IndicesOffset;
		jobData3.AllowMerging = flag;
		jobData3.DrawOneByOne = ShouldDrawOneByOne();
		GPUDrivenBRGDebug debugData = m_BRG.DebugData;
		jobData3.DisableDecalSorting = debugData != null && !debugData.DecalSorting;
		debugData = m_BRG.DebugData;
		jobData3.DisableDecalBatching = debugData != null && !debugData.DecalBatching;
		jobData3.RendererGroups = m_RendererGroups;
		jobData3.Materials = m_ResourceRegistry.GetInnerUnmanagedMaterialPool();
		jobData3.RendererGroupIndices = m_RendererGroupIndices;
		jobData3.SortableGroupDescriptions = sortableGroupDescriptions;
		jobData3.InstanceMetadata = m_BRG.GetAllInstanceMetadataReadonly();
		jobData3.GroupMergeabilityMask = nativeArray;
		jobData3.AllIndices = allIndices;
		jobData3.RendererGroupSlices = rendererGroupSlices;
		jobData3.RendererGroupSlicesCounter = new UnsafeAtomicCounter32(pSlicesCount);
		jobData3.IndicesCounter = new UnsafeAtomicCounter32(pIndexCount);
		dependsOn = jobData3.ScheduleBatch(sortableGroupDescriptions.Length, 256, dependsOn);
		sortableGroupDescriptionJob.Dispose(dependsOn);
		nativeArray.Dispose(dependsOn);
		nativeList.Dispose(dependsOn);
		return dependsOn;
	}

	private bool ShouldDrawOneByOne()
	{
		GPUDrivenBRGDebug debugData = m_BRG.DebugData;
		if (debugData == null || !debugData.DrawOneByOne)
		{
			WaaaghDebugData waaaghDebugData = m_WaaaghDebugData;
			if ((object)waaaghDebugData != null)
			{
				RenderingDebug renderingDebug = waaaghDebugData.RenderingDebug;
				if (renderingDebug != null)
				{
					return renderingDebug.OverdrawMode == DebugOverdrawMode.QuadOverdraw;
				}
			}
			return false;
		}
		return true;
	}

	private unsafe JobHandle ScheduleFillGroupInfoAndGPUCullingJobs(ViewType viewType, int* pCullingJobsCount)
	{
		JobHandle jobHandle = default(JobHandle);
		ref ViewTypeInfo reference = ref ViewTypeInfos[(int)viewType];
		NativeSlice<RendererGroupSlice> rendererGroupSlices = m_RendererGroupSlices.Slice(reference.RendererGroupSlicesOffset, reference.RendererGroupSlicesCount);
		NativeSlice<GPUDrivenComputeShaders.GroupInfo> groupInfos = m_GroupInfos.Slice(reference.GroupInfoOffset, reference.GroupInfoMaxCount);
		NativeSlice<GPUDrivenComputeShaders.GPUCullingJob> gPUCullingJobs = m_GPUCullingJobs.Slice(reference.GPUCullingJobsOffset, reference.GPUCullingJobsMaxCount);
		FillGroupInfoJob jobData = default(FillGroupInfoJob);
		jobData.RendererGroups = m_RendererGroups;
		jobData.RendererGroupSlices = rendererGroupSlices;
		jobData.Meshes = m_ResourceRegistry.GetInnerUnmanagedMeshPool();
		jobData.GroupInfos = groupInfos;
		return JobHandle.CombineDependencies(jobData.Schedule(reference.RendererGroupSlicesCount, jobHandle), new CreateGPUCullingJobsJob
		{
			WrittenGroupOffset = reference.RendererGroupSlicesOffset,
			RendererGroupSlices = rendererGroupSlices,
			RendererGroups = m_RendererGroups,
			Materials = m_ResourceRegistry.GetInnerUnmanagedMaterialPool(),
			GPUCullingJobs = gPUCullingJobs,
			CullingJobsCountPtr = pCullingJobsCount
		}.Schedule(jobHandle));
	}

	public unsafe void BeginRebuildingGroupInfo()
	{
		using (new ProfilingScope(Profiling.BeginRebuildingGroupInfo))
		{
			EnsureGroupsAreSorted();
			GPUDrivenSortableGroupDescriptionManager.GetValueArrayJobInfo<GPUDrivenIndexAllocator.IndexAllocation> groupAllocationsValueArray = default(GPUDrivenSortableGroupDescriptionManager.GetValueArrayJobInfo<GPUDrivenIndexAllocator.IndexAllocation>);
			NativeList<GPUDrivenSortableGroupDescriptionManager.JobInfo> nativeArray;
			using (new ProfilingScope(Profiling.StartSortableGroupDescriptionJobs))
			{
				nativeArray = new NativeList<GPUDrivenSortableGroupDescriptionManager.JobInfo>(Allocator.Temp);
				for (int i = 0; i < ViewTypeInfos.Length; i++)
				{
					ref ViewTypeInfo reference = ref ViewTypeInfos[i];
					ref PendingGroupRebuild pendingRebuild = ref reference.PendingRebuild;
					if (pendingRebuild.InProgress || (reference.GroupSlicesDirtyState != DirtyState.DirtyOnCPU && reference.GroupSlicesDirtyState != DirtyState.DirtyOnCPU))
					{
						continue;
					}
					pendingRebuild = new PendingGroupRebuild
					{
						InProgress = true
					};
					m_BRG.InstanceCuller.InvalidateCommandCache(reference.ViewType);
					NativeList<GPUDrivenIndexAllocator.IndexAllocation> emptyGroupIndices = default(NativeList<GPUDrivenIndexAllocator.IndexAllocation>);
					if (i == 0)
					{
						m_EmptyGroupIndices.Clear();
						int count = m_RendererGroupKeyToIndex.Count;
						if (m_EmptyGroupIndices.Capacity < count)
						{
							m_EmptyGroupIndices.SetCapacity(count);
						}
						emptyGroupIndices = m_EmptyGroupIndices;
					}
					pendingRebuild.TempSliceCount = new NativeArray<int>(1, Allocator.TempJob);
					pendingRebuild.TempIndexCount = new NativeArray<int>(1, Allocator.TempJob);
					GPUDrivenSortableGroupDescriptionManager.JobInfo value = m_SortableGroupDescriptionManager.Collect(reference.ViewType, emptyGroupIndices, ref groupAllocationsValueArray, Allocator.TempJob);
					nativeArray.Add(in value);
				}
			}
			if (nativeArray.Length <= 0)
			{
				return;
			}
			using (new ProfilingScope(Profiling.ScheduleSortGroupsAndPopulateSlices))
			{
				nativeArray.Sort(default(GPUDrivenSortableGroupDescriptionManager.JobInfo.ExpectedJobDurationComparer));
				for (int j = 0; j < nativeArray.Length; j++)
				{
					ref GPUDrivenSortableGroupDescriptionManager.JobInfo reference2 = ref UnsafeCollectionExtensions.ElementAsRef(in nativeArray, j);
					ref PendingGroupRebuild pendingRebuild2 = ref ViewTypeInfos[(int)reference2.ViewType].PendingRebuild;
					pendingRebuild2.SliceBuildingJobHandle = ScheduleSortGroupsAndPopulateSlices(reference2, (int*)pendingRebuild2.TempSliceCount.GetUnsafePtr(), (int*)pendingRebuild2.TempIndexCount.GetUnsafePtr());
				}
			}
			groupAllocationsValueArray.Dispose();
		}
	}

	private void EnsureGroupsAreSorted()
	{
		if (m_RendererGroupsToSort.Length == 0)
		{
			return;
		}
		NativeList<GPUDrivenIndexAllocator.IndexAllocation> nativeList = new NativeList<GPUDrivenIndexAllocator.IndexAllocation>(m_RendererGroupsToSort.Length, Allocator.TempJob);
		foreach (GPUDrivenIndexAllocator.IndexAllocation item in m_RendererGroupsToSort)
		{
			GPUDrivenIndexAllocator.IndexAllocation value = item;
			ref RendererGroup reference = ref UnsafeCollectionExtensions.ElementAsRef(in m_RendererGroups, value.Index);
			if (reference.PendingSorting)
			{
				nativeList.Add(in value);
				reference.PendingSorting = false;
			}
		}
		m_RendererGroupsToSort.Clear();
		if (nativeList.Length == 0)
		{
			return;
		}
		using (new ProfilingScope(Profiling.SortGroupInstanceIndices))
		{
			SortGroupInstanceIndicesJob jobData = default(SortGroupInstanceIndicesJob);
			jobData.InstanceMetadata = m_BRG.GetAllInstanceMetadataReadonly();
			jobData.Groups = nativeList.AsArray();
			jobData.GroupIndices = m_RendererGroupIndices;
			JobHandle inputDeps = jobData.Schedule(nativeList.Length, default(JobHandle));
			nativeList.Dispose(inputDeps);
			inputDeps.Complete();
		}
	}

	public unsafe void CompleteGroupSliceBuilding(ViewType viewType)
	{
		ref ViewTypeInfo reference = ref ViewTypeInfos[(int)viewType];
		ref PendingGroupRebuild pendingRebuild = ref reference.PendingRebuild;
		if (!pendingRebuild.InProgress)
		{
			return;
		}
		using (new ProfilingScope(Profiling.CompleteRebuildingGroupSlices))
		{
			pendingRebuild.SliceBuildingJobHandle?.Complete();
			reference.RendererGroupSlicesCount = (reference.GroupInfoCount = pendingRebuild.TempSliceCount[0]);
			reference.IndicesCount = pendingRebuild.TempIndexCount[0];
			using (new ProfilingScope(Profiling.ScheduleFillGroupInfoAndGPUCullingJobs))
			{
				pendingRebuild.TempGPUCullingJobsCount = new NativeArray<int>(1, Allocator.TempJob);
				pendingRebuild.GPUCullingJobsJobHandle = ScheduleFillGroupInfoAndGPUCullingJobs(reference.ViewType, (int*)pendingRebuild.TempGPUCullingJobsCount.GetUnsafePtr());
			}
			reference.GroupSlicesDirtyState = DirtyState.DirtyOnGPU;
		}
	}

	private void CompleteAllRebuilding(ViewType viewType)
	{
		ref ViewTypeInfo reference = ref ViewTypeInfos[(int)viewType];
		ref PendingGroupRebuild pendingRebuild = ref reference.PendingRebuild;
		if (!pendingRebuild.InProgress)
		{
			return;
		}
		if (!pendingRebuild.GPUCullingJobsJobHandle.HasValue)
		{
			CompleteGroupSliceBuilding(viewType);
		}
		using (new ProfilingScope(Profiling.CompleteRebuildingGroupInfo))
		{
			pendingRebuild.GPUCullingJobsJobHandle?.Complete();
			reference.GPUCullingJobsCount = pendingRebuild.TempGPUCullingJobsCount[0];
			reference.PendingRebuild.Dispose();
			reference.PendingRebuild = default(PendingGroupRebuild);
			reference.GPUCullingJobsDirtyState = DirtyState.DirtyOnGPU;
		}
	}

	public void OnModifiedCPUData()
	{
		for (int i = 0; i < ViewTypeInfos.Length; i++)
		{
			ref ViewTypeInfo reference = ref ViewTypeInfos[i];
			reference.GPUCullingJobsDirtyState = (reference.GroupSlicesDirtyState = DirtyState.DirtyOnCPU);
		}
	}

	public void EarlyPostRender()
	{
		using (new ProfilingScope(Profiling.PostRender))
		{
			ViewTypeInfo[] viewTypeInfos = ViewTypeInfos;
			for (int i = 0; i < viewTypeInfos.Length; i++)
			{
				ViewTypeInfo viewTypeInfo = viewTypeInfos[i];
				CompleteAllRebuilding(viewTypeInfo.ViewType);
			}
		}
	}

	public void PostRender()
	{
		using (new ProfilingScope(Profiling.PostRender))
		{
			foreach (GPUDrivenIndexAllocator.IndexAllocation emptyGroupIndex in m_EmptyGroupIndices)
			{
				FreeGroup(ref Get(emptyGroupIndex.Index));
			}
			m_EmptyGroupIndices.Clear();
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static int CompareInstances(in GPUDrivenBatchRendererGroup.InstanceMetadata metadata1, in GPUDrivenBatchRendererGroup.InstanceMetadata metadata2)
	{
		int sortingOrder = metadata1.SortingOrder;
		int num = sortingOrder.CompareTo(metadata2.SortingOrder);
		if (num != 0)
		{
			return num;
		}
		ulong serialNumber = metadata1.SerialNumber;
		return serialNumber.CompareTo(metadata2.SerialNumber);
	}

	public void ScheduleSorting(ref RendererGroup rendererGroup)
	{
		if (!rendererGroup.PendingSorting && m_ResourceRegistry.GetUnmanagedMaterialInfo(in rendererGroup).GPUDrivenMaterialUniquenessKey.RequiresCustomSorting())
		{
			rendererGroup.PendingSorting = true;
			m_RendererGroupsToSort.Add(in rendererGroup.IndexAllocation);
			OnModifiedCPUData();
		}
	}
}
