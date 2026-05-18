using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public struct RendererSetupContext
{
	public ScriptableRenderContext ScriptableRenderContext;

	public WaaaghCameraData CameraData;

	public WaaaghRenderingData RenderingData;

	public WaaaghShadowData ShadowData;
}
