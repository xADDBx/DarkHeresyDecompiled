using System;
using JetBrains.Annotations;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\GPUDrivenBRG\\GPUDrivenComputeShaders.cs")]
public static class GPUDrivenComputeShaders
{
	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\GPUDrivenBRG\\GPUDrivenComputeShaders.cs")]
	[Flags]
	public enum GPUCullingJobFlags
	{
		None = 0,
		TransparentMaterial = 1
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\GPUDrivenBRG\\GPUDrivenComputeShaders.cs", needAccessors = false)]
	public struct GroupInfo
	{
		public uint MeshIndexStart;

		public uint MeshIndexCount;

		public uint PersistentInstanceIndexStart;

		public uint VisibleInstanceIndexStart;

		public uint InstanceIndexCount;
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\GPUDrivenBRG\\GPUDrivenComputeShaders.cs", needAccessors = false)]
	public struct GPUCullingContext
	{
		public float4 CullingSphereLS;

		public float4x4 CullingMatrix;

		public float4x4 WorldToLightSpaceRotation;

		public float4 CameraPosition;

		public uint BatchCullingViewType;

		public uint PersistentIndicesOffset;

		public uint VisibleIndicesOffset;

		public uint FrustumPlanesOffset;

		public uint FrustumPlanesCount;

		public uint GroupCountersOffset;

		public uint SceneCullingMaskLow;

		public uint SceneCullingMaskHigh;

		public uint IndirectArgsOffset;

		public uint CPUInstanceVisibilityMaskOffset;

		public uint CullingJobsOffset;

		public uint GroupsOffset;

		public uint GroupCount;

		public CameraType CameraType;

		public uint LOD_IsOrtho;

		public uint LOD_UseSelectionForcedLOD;

		public float LOD_SqrScreenRelativeMetric;

		public float3 LOD_CameraPosition;

		public int LOD_MaxLOD;

		public int LOD_FixedLODIndex;

		public float LOD_LODBias;
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@2c5e70bf14b9\\Runtime\\GPUDrivenBRG\\GPUDrivenComputeShaders.cs", needAccessors = false)]
	public struct GPUCullingJob
	{
		public uint GroupIndex;

		public uint Offset;

		public uint Count;

		public GPUCullingJobFlags Flags;
	}

	[UsedImplicitly]
	public const int kBRGCullingGroupSize = 32;

	[UsedImplicitly]
	public const int kRawBufferClearGroupSize = 32;

	[UsedImplicitly]
	public const int kRawBufferCopyGroupSize = 32;

	[UsedImplicitly]
	public const int kDataUploadGroupSize = 256;

	[UsedImplicitly]
	public const int kBRGFixupIndirectArgsGroupSize = 32;

	[UsedImplicitly]
	public const int kUpdateInstancesCreatedThisFrameGroupSize = 32;

	[UsedImplicitly]
	public const int kFindForwardReflectionProbes = 32;

	[UsedImplicitly]
	public const int kMaxCullingContextsPerDispatch = 16;

	[UsedImplicitly]
	public const int kDepthReprojectionGroupSize = 32;

	public static int ComputeGroupCount(int threadsCount, int groupSize)
	{
		return (int)math.ceil((float)threadsCount / (float)groupSize);
	}
}
