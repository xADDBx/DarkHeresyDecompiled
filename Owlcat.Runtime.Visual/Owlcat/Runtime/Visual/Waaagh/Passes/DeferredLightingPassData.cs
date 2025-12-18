using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DeferredLightingPassData : PassDataBase
{
	public TextureHandle CameraColorRT;

	public TextureHandle CameraDepthRT;

	public TextureHandle CameraDepthCopytRT;

	public TextureHandle CameraAlbedoRT;

	public TextureHandle CameraNormalsRT;

	public TextureHandle CameraBakedGIRT;

	public TextureHandle CameraShadowmaskRT;

	public TextureHandle CameraTranslucencyRT;

	public Material DeferredLightingMaterial;

	public bool SsrEnabled;

	public Color GlossyEnvironmentColor;

	public Color GlossyBlackColor;
}
