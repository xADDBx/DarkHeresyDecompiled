using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh;

public struct RendererRecordContext
{
	public RenderGraph RenderGraph;

	public WaaaghRenderingData RenderingData;

	public WaaaghCameraData CameraData;

	public VirtualTextureManager VirtualTextureManager;

	public WaaaghShadowData ShadowData;

	public GPUDrivenBatchRendererGroup GPUDrivenBatchRendererGroup;

	public DebugContext DebugContext;

	public bool IsVTEnabled => VirtualTextureManager != null;
}
