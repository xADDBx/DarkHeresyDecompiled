using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTerrain;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct VirtualTerrainShader
{
	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\VirtualTerrain\\VirtualTerrainShader.cs")]
	public static class Constants
	{
		public const int kVirtualTerrainLayerCountMax = 64;

		public const float kVirtualTerrainMipBias = -0.5f;

		public const float kVirtualTerrainMipBiasFactor = 0.70710677f;

		public const int kVirtualTerrainPageTypeCount = 3;

		public const int kVirtualTerrainPageMipCount = 3;

		public const int kVirtualTerrainPageLodCount = 3;

		public const int kVirtualTerrainRedirectionLength = 384;

		public const int kVirtualTerrainStampingWeightsLength = 32;
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\VirtualTerrain\\VirtualTerrainShader.cs", generateCBuffer = true, needAccessors = false)]
	public struct UnityPerMaterial
	{
		public Vector4 _BaseMap_ST;

		public Vector4 _MainTex_ST;

		public Vector4 _SplatArray_ST;

		public Vector4 _SplatArray_TexelSize;

		public Vector4 _DiffuseArray_TexelSize;

		public Vector4 _TransitionDiffuseArray_TexelSize;

		public Vector4 _Color;

		public Vector4 _VirtualTerrainSplatMap_TexelSize;

		public Vector4 _VirtualTerrainLayerParams;

		public float _AlphaGain;

		public float _AlphaBlendFactor;

		public float _TerrainMaxHeight;

		public float _Pad;

		[HLSLArray(384, typeof(Vector4))]
		public unsafe fixed float _VirtualTerrainRedirection[1536];

		[HLSLArray(32, typeof(Vector4))]
		public unsafe fixed float _VirtualTerrainStampingWeights[128];
	}

	[GenerateHLSL(PackingRules.Exact, true, false, false, 1, false, false, false, -1, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\VirtualTerrain\\VirtualTerrainShader.cs", generateCBuffer = true, needAccessors = false)]
	public struct VirtualTerrainGlobals
	{
		public Vector4 _VirtualTerrainAtlasParams;
	}

	public static class Properties
	{
		public static readonly int LayerParams = Shader.PropertyToID("_VirtualTerrainLayerParams");

		public static readonly int Redirection = Shader.PropertyToID("_VirtualTerrainRedirection");

		public static readonly int StampingWeights = Shader.PropertyToID("_VirtualTerrainStampingWeights");

		public static readonly int SplatMap = Shader.PropertyToID("_VirtualTerrainSplatMap");

		public static readonly int TerrainMaxHeight = Shader.PropertyToID("_TerrainMaxHeight");

		public static readonly int DebugHeatMap = Shader.PropertyToID("_DebugTerrainHeatMap");
	}
}
