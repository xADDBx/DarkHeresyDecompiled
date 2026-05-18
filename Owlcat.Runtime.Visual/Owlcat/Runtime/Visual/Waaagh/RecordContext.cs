using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.Recorders.Debugging;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh;

public struct RecordContext
{
	public RenderGraph RenderGraph;

	public WaaaghRenderingData RenderingData;

	public WaaaghCameraData CameraData;

	public VirtualTextureManager VirtualTextureManager;

	public WaaaghShadowData ShadowData;

	public GPUDrivenBatchRendererGroup GPUDrivenBatchRendererGroup;

	public DebugContext DebugContext;

	public WaaaghLights Lights;

	public RenderRuntimeShaders Shaders;

	public RenderRuntimeTextures Textures;

	public WaaaghReflectionProbes ReflectionProbes;

	public DeferredReflectionProbeBatcher DeferredReflectionProbeBatcher;

	public FrameResources FrameResources;

	public MaterialLibrary MaterialLibrary;

	public bool IsVTEnabled => VirtualTextureManager != null;
}
