using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[Serializable]
public sealed class GPUDrivenBRGSettings
{
	[NonSerialized]
	private static bool? s_IsSupported;

	[SerializeField]
	[FormerlySerializedAs("Enabled")]
	private bool m_Enabled;

	[Header("Sorting")]
	public bool OpaqueSortingCPU;

	public bool OpaqueSortingGPU;

	[Header("Occlusion Culling")]
	public bool OcclusionCulling;

	public bool OccludeeUseAABB;

	public bool DepthReprojection;

	[Range(-0.01f, 0.01f)]
	public float DepthReprojectionBias = 0.001f;

	[Tooltip("Fetch readback visibility mask to cull draw calls on CPU.")]
	public GPUDrivenVisibilityMaskReadbackMode VisibilityMaskReadbackMode;

	[Header("Batching")]
	[Tooltip("Disabled = no material sorting and merging.\nCoarse Sort = sort by basic info like shader, keywords, and mesh.\nSegment Sort = sort by most info but in several independent segments.\nFine Sort = sort by most info.\nExact Sort = precise sorting, considers all info.")]
	public GPUDrivenBRGMaterialMergeMode MaterialMergeMode = GPUDrivenBRGMaterialMergeMode.FineSort;

	public GPUDrivenBRGMaterialMergeMode DepthOnlyMaterialMergeMode = GPUDrivenBRGMaterialMergeMode.FineSort;

	[Header("Memory")]
	[Min(1f)]
	public int InitialInstanceCapacity = 128;

	[Min(1f)]
	public int InitialMaterialCapacity = 16;

	[Min(1f)]
	public int ReferenceMaterialSizeInBytes = 64;

	[Min(10240f)]
	public int MaxPersistentBufferCapacityInBytes = 1048576;

	[Min(1f)]
	public int InitialRendererGroupsCapacity = 16;

	[Min(1f)]
	public int InitialScenesCapacity = 4;

	[Min(1f)]
	public int InitialCullingSplitsCapacity = 8;

	public GPUUploadMode UploadMode;

	[Header("Misc")]
	public bool UseForIndirectRenderingSystem;

	public bool IsEnabledAndSupported
	{
		get
		{
			bool valueOrDefault = s_IsSupported.GetValueOrDefault();
			if (!s_IsSupported.HasValue)
			{
				valueOrDefault = IsSupported();
				s_IsSupported = valueOrDefault;
			}
			if (m_Enabled)
			{
				return s_IsSupported.Value;
			}
			return false;
		}
	}

	public bool SetEnabled(bool enabled)
	{
		if (m_Enabled == enabled)
		{
			return false;
		}
		m_Enabled = enabled;
		return true;
	}

	internal static bool HasSupportChanged()
	{
		return s_IsSupported != IsSupported();
	}

	internal static void ResetSupport()
	{
		s_IsSupported = null;
	}

	private static bool IsSupported()
	{
		return true;
	}
}
