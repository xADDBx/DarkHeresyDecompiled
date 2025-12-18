using System;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.Debugging;

[Serializable]
public class GPUDrivenBRGDebug
{
	public enum DataUploadDebugMode
	{
		None,
		UploadFull,
		UploadManySmallSegments
	}

	public bool OverrideCameraToMain;

	public bool SkipRendering;

	public bool SkipSubmittingDrawCommands;

	public bool DrawOneByOne;

	public bool DecalSorting = true;

	public bool DecalBatching = true;

	public bool DisableGroupSorting;

	public bool DisableGroupMerging;

	public bool ForceBuildRenderGroupsEachFrame;

	public BatchCullingViewType ViewTypeFilter;

	public DataUploadDebugMode GPUDataUploadDebugMode;

	public bool ShowOcclusionTest;

	public int OcclusionTestCountRange = 32;

	public float OcclusionTestOpacity = 0.5f;

	public bool CullingStats;
}
