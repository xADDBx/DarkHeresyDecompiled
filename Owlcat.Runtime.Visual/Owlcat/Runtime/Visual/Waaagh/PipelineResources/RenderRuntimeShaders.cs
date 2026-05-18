using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PipelineResources;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "R: Waaagh Renderer Runtime Shaders", Order = 1000)]
public class RenderRuntimeShaders : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[ResourcePath("Shaders/Utils/CopyDepth.shader", SearchType.ProjectPath)]
	public Shader CopyDepthPS;

	[ResourcePath("Shaders/Utils/CopyDepthSimple.shader", SearchType.ProjectPath)]
	public Shader CopyDepthSimplePS;

	[ResourcePath("Shaders/Utils/StencilMask.shader", SearchType.ProjectPath)]
	public Shader StencilMaskPS;

	[ResourcePath("Shaders/Utils/DepthPyramid.compute", SearchType.ProjectPath)]
	public ComputeShader DepthPyramidCS;

	[ResourcePath("Shaders/Utils/DepthPyramidFfxSpd.compute", SearchType.ProjectPath)]
	public ComputeShader DepthPyramidFfxSpdCS;

	[ResourcePath("Shaders/PostProcessing/EdgeAdaptiveSpatialUpsampling.shader", SearchType.ProjectPath)]
	public Shader FsrEasuShader;

	[ResourcePath("Runtime/Waaagh/Lighting/LightCulling.compute", SearchType.ProjectPath)]
	public ComputeShader LightCullingShader;

	[ResourcePath("Runtime/Waaagh/Lighting/ComputeTilesMinMaxZ.compute", SearchType.ProjectPath)]
	public ComputeShader ComputeTilesMinMaxZCS;

	[ResourcePath("Runtime/Waaagh/Lighting/DeferredReflections.shader", SearchType.ProjectPath)]
	public Shader DeferredReflectionsShader;

	[ResourcePath("Runtime/Waaagh/Lighting/DeferredReflections.mat", SearchType.ProjectPath)]
	public Material DeferredReflectionsMaterial;

	[ResourcePath("Runtime/Waaagh/Lighting/DeferredLighting.shader", SearchType.ProjectPath)]
	public Shader DeferredLightingShader;

	[ResourcePath("Runtime/Waaagh/Lighting/DeferredLighting.compute", SearchType.ProjectPath)]
	public ComputeShader DeferredLightingCS;

	[ResourcePath("Runtime/Waaagh/Lighting/DeferredLightingDxc.compute", SearchType.ProjectPath)]
	public ComputeShader DeferredLightingDxcCS;

	[ResourcePath("Runtime/Waaagh/Lighting/BuildFeatureTiles.compute", SearchType.ProjectPath)]
	public ComputeShader DeferredLightingBuildFeatureTilesCS;

	[ResourcePath("Runtime/Waaagh/Lighting/BuildFeatureTilesLists.compute", SearchType.ProjectPath)]
	public ComputeShader DeferredLightingBuildFeatureTilesListsCS;

	[ResourcePath("Shaders/Utils/Blit.shader", SearchType.ProjectPath)]
	public Shader BlitShader;

	[ResourcePath("Shaders/Utils/ColorPyramid.shader", SearchType.ProjectPath)]
	public Shader ColorPyramidShader;

	[ResourcePath("Shaders/Utils/ApplyDistortion.shader", SearchType.ProjectPath)]
	public Shader ApplyDistortionShader;

	[ResourcePath("Shaders/Utils/DBufferBlit.shader", SearchType.ProjectPath)]
	public Shader DBufferBlitShader;

	[ResourcePath("Shaders/Utils/Fog.shader", SearchType.ProjectPath)]
	public Shader FogShader;

	[ResourcePath("Runtime/IndirectRendering/IndirectCulling.compute", SearchType.ProjectPath)]
	public ComputeShader IndirectRenderingCullShader;

	[ResourcePath("Shaders/PostProcessing/ScreenSpaceReflections/ScreenSpaceReflections.compute", SearchType.ProjectPath)]
	public ComputeShader ScreenSpaceReflectionsShaderCS;

	[ResourcePath("Shaders/PostProcessing/ScreenSpaceReflections/WaaaghSSR.shader", SearchType.ProjectPath)]
	public Shader ScreenSpaceReflectionsShaderPS;

	[ResourcePath("Shaders/PostProcessing/ScreenSpaceReflections/StochasticSSR.compute", SearchType.ProjectPath)]
	public ComputeShader StochasticScreenSpaceReflectionsCS;

	[ResourcePath("Runtime/Waaagh/BilateralUpsample/BilateralUpsample.compute", SearchType.ProjectPath)]
	public ComputeShader BilateralUpsampleCS;

	[ResourcePath("Shaders/PostProcessing/CameraMotionVectors.shader", SearchType.ProjectPath)]
	public Shader CameraMotionVectorsPS;

	[ResourcePath("Shaders/PostProcessing/ObjectMotionVectors.shader", SearchType.ProjectPath)]
	public Shader ObjectMotionVectorsPS;

	[ResourcePath("Shaders/Utils/CoreBlit.shader", SearchType.ProjectPath)]
	public Shader CoreBlitPS;

	[ResourcePath("Shaders/Utils/BlitHDROverlay.shader", SearchType.ProjectPath)]
	public Shader BlitHDROverlayPS;

	[ResourcePath("Shaders/Utils/CoreBlitColorAndDepth.shader", SearchType.ProjectPath)]
	public Shader CoreBlitColorAndDepthPS;

	[ResourcePath("Shaders/Utils/MedianBlur.compute", SearchType.ProjectPath)]
	public ComputeShader MedianBlurCS;

	[ResourcePath("Shaders/Utils/CopyCachedShadows.shader", SearchType.ProjectPath)]
	public Shader CopyCachedShadowsPS;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
}
