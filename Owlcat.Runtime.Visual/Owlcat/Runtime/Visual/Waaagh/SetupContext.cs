using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public struct SetupContext
{
	public ScriptableRenderContext ScriptableRenderContext;

	public WaaaghCameraData CameraData;

	public WaaaghRenderingData RenderingData;

	public WaaaghShadowData ShadowData;

	public WaaaghLights Lights;
}
