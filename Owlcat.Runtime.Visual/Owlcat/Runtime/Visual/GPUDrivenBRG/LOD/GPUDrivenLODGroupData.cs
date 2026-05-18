using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.LOD;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\GPUDrivenBRG\\LOD\\GPUDrivenLODGroupData.cs", needAccessors = false)]
public struct GPUDrivenLODGroupData
{
	[UsedImplicitly]
	public const int kMaxLODLevelsCount = 8;

	public Vector3 WorldSpaceReferencePoint;

	public float WorldSpaceSize;

	public int LODCount;

	public LODFadeMode FadeMode;

	public uint SelectionForcedLOD;

	public uint AnimatedCrossFade;

	[HLSLArray(8, typeof(float))]
	public unsafe fixed float Distances[8];

	[HLSLArray(8, typeof(float))]
	public unsafe fixed float TransitionDistances[8];
}
