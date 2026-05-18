using UnityEngine;

namespace Owlcat.Runtime.Visual;

public static class GlobalTextureShaderPropertyId
{
	public static readonly int _CameraAlbedoRT = Shader.PropertyToID("_CameraAlbedoRT");

	public static readonly int _CameraSpecularRT = Shader.PropertyToID("_CameraSpecularRT");

	public static readonly int _CameraNormalsRT = Shader.PropertyToID("_CameraNormalsRT");

	public static readonly int _CameraNormalsTexture = Shader.PropertyToID("_CameraNormalsTexture");

	public static readonly int _CameraTranslucencyRT = Shader.PropertyToID("_CameraTranslucencyRT");

	public static readonly int _CameraBakedGIRT = Shader.PropertyToID("_CameraBakedGIRT");

	public static readonly int _CameraShadowmaskRT = Shader.PropertyToID("_CameraShadowmaskRT");

	public static readonly int _ShadowmapRT = Shader.PropertyToID("_ShadowmapRT");

	public static readonly int _CameraDepthPyramidRT = Shader.PropertyToID("_CameraDepthPyramidRT");

	public static readonly int _CameraColorPyramidRT = Shader.PropertyToID("_CameraColorPyramidRT");

	public static readonly int _DecalsNormalsRT = Shader.PropertyToID("_DecalsNormalsRT");

	public static readonly int _DecalsMasksRT = Shader.PropertyToID("_DecalsMasksRT");

	public static readonly int _CameraDepthTexture = Shader.PropertyToID("_CameraDepthTexture");

	public static readonly int _CameraDepthRT = Shader.PropertyToID("_CameraDepthRT");

	public static readonly int _TilesMinMaxZTexture = Shader.PropertyToID("_TilesMinMaxZTexture");

	public static readonly int _ScreenSpaceOcclusionTexture = Shader.PropertyToID("_ScreenSpaceOcclusionTexture");

	public static readonly int _VTFeedbackMRT = Shader.PropertyToID("_VTFeedbackMRT");

	public static readonly int waaagh_ReflProbes_Atlas = Shader.PropertyToID("waaagh_ReflProbes_Atlas");
}
