using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\WaaaghShaderVariablesGlobal.cs", needAccessors = false, generateCBuffer = true, packingRules = PackingRules.Exact)]
public struct WaaaghShaderVariablesGlobal
{
	public const int kMaxZBinVec4s = 1024;

	public const int kMaxReflectionProbes = 64;

	public const int kReflProbeMipLevels = 7;

	public float4 _VTPageParams;

	public float4 _VTTileParams;

	public float4 _VTPhysicalAtlasSize;

	public float _VTFeedbackMipBias;

	public float _VTPad_a;

	public float _VTPad_b;

	public float _VTPad_c;

	[HLSLArray(1024, typeof(Vector4))]
	public unsafe fixed float _ZBins[4096];

	public const int kTranslucencyProfileMaxCount = 16;

	public float4 _TranslucencyProfileCount;

	[HLSLArray(16, typeof(Vector4))]
	public unsafe fixed float _TranslucencyProfileColors[64];

	[HLSLArray(16, typeof(Vector4))]
	public unsafe fixed float _TranslucencyProfileSelfShadowingWorkaroundData[64];

	public const int kFaceVectorsCount = 4;

	public float4 _ShadowAtlasSize;

	public float2 _ShadowFadeDistanceScaleAndBias;

	public float _GlobalShadowsEnabled;

	public float _ShadowReceiverNormalBias;

	public float _DirectionalCascadesCount;

	public float _ShadowsPad_a;

	public float _ShadowsPad_b;

	public float _ShadowsPad_c;

	[HLSLArray(4, typeof(Vector4))]
	public unsafe fixed float _FaceVectors[16];

	public float waaagh_ReflProbes_Count;

	public float _ReflProbesPad_a;

	public float _ReflProbesPad_b;

	public float _ReflProbesPad_c;

	[HLSLArray(64, typeof(Vector4))]
	public unsafe fixed float waaagh_ReflProbes_BoxMax[256];

	[HLSLArray(64, typeof(Vector4))]
	public unsafe fixed float waaagh_ReflProbes_BoxMin[256];

	[HLSLArray(64, typeof(Vector4))]
	public unsafe fixed float waaagh_ReflProbes_ProbePosition[256];

	[HLSLArray(448, typeof(Vector4))]
	public unsafe fixed float waaagh_ReflProbes_MipScaleOffset[1792];
}
