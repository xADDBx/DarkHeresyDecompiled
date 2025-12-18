using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@01c4fcbc474f\\Runtime\\GPUDrivenBRG\\GPUDrivenVisibilityInfo.cs", needAccessors = false)]
public struct GPUDrivenVisibilityInfo
{
	public float4 BoundingSphere;

	public float4 AABBExtents;

	public GPUDrivenVisibilityFlags VisibilityFlags;

	public GPUDrivenDynamicFlags DynamicFlags;

	public uint GameObjectLayerMask;

	public uint SceneCullingMaskLow;

	public uint SceneCullingMaskHigh;

	public uint PackedLODInstanceInfo;

	public uint Padding0;

	public uint Padding1;
}
