using System.Runtime.InteropServices;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Terrain;

[StructLayout(LayoutKind.Sequential, Size = 1)]
internal struct OwlcatTerrainShader
{
	public static class Keywords
	{
		public const string TerrainTransition = "_TERRAIN_TRANSITION";

		public const string TerrainTransitionBlendMask = "_TERRAIN_TRANSITION_BLEND_MASK";

		public const string TerrainTransitionBlendDithering = "_TERRAIN_TRANSITION_BLEND_DITHERING";

		public const string TerrainStamping = "_TERRAIN_STAMPING";
	}

	public static readonly int TerrainLayerMasksScale = Shader.PropertyToID("_TerrainLayerMasksScale");

	public static readonly int TerrainLayerUvMatrix = Shader.PropertyToID("_TerrainLayerUvMatrix");

	public static readonly int TerrainLayerParams = Shader.PropertyToID("_TerrainLayerParams");

	public static readonly int TerrainLayerStampingWeights = Shader.PropertyToID("_TerrainLayerStampingWeights");

	public static readonly int DiffuseArray = Shader.PropertyToID("_DiffuseArray");

	public static readonly int NormalArray = Shader.PropertyToID("_NormalArray");

	public static readonly int MasksArray = Shader.PropertyToID("_MasksArray");

	public static readonly int SplatArray = Shader.PropertyToID("_SplatArray");

	public static readonly int ControlTexturesCount = Shader.PropertyToID("_ControlTexturesCount");

	public static readonly int TerrainMaxHeight = Shader.PropertyToID("_TerrainMaxHeight");

	public static readonly int AlphaGain = Shader.PropertyToID("_AlphaGain");

	public static readonly int AlphaBlendFactor = Shader.PropertyToID("_AlphaBlendFactor");

	public static readonly int TerrainTransitionShape = Shader.PropertyToID("_TerrainTransitionShape");

	public static readonly int TerrainTransitionCustomClipParams = Shader.PropertyToID("_TerrainTransitionCustomClipParams");

	public static readonly int TerrainTransitionBlendMask = Shader.PropertyToID("_TerrainTransitionBlendMask");

	public static readonly int TerrainTransitionBlendMaskScaleOffset = Shader.PropertyToID("_TerrainTransitionBlendMask_ST");

	public static readonly int TransitionTerrainLayerMasksScale = Shader.PropertyToID("_TransitionTerrainLayerMasksScale");

	public static readonly int TransitionTerrainLayerUvMatrix = Shader.PropertyToID("_TransitionTerrainLayerUvMatrix");

	public static readonly int TransitionTerrainLayerParams = Shader.PropertyToID("_TransitionTerrainLayerParams");

	public static readonly int TransitionTerrainLayerStampingWeights = Shader.PropertyToID("_TransitionTerrainLayerStampingWeights");

	public static readonly int TransitionDiffuseArray = Shader.PropertyToID("_TransitionDiffuseArray");

	public static readonly int TransitionNormalArray = Shader.PropertyToID("_TransitionNormalArray");

	public static readonly int TransitionMasksArray = Shader.PropertyToID("_TransitionMasksArray");

	public static readonly int DebugTerrainOverlay = Shader.PropertyToID("_DebugTerrainOverlay");
}
