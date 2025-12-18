using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PipelineResources;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "R: PostProcess Runtime Shaders", Order = 1000)]
public class PostProcessRuntimeShaders : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version = 1;

	[ResourcePath("Shaders/PostProcessing/LutBuilderLdr.shader", SearchType.ProjectPath)]
	public Shader LutBuilderLdrPS;

	[ResourcePath("Shaders/PostProcessing/LutBuilderHdr.shader", SearchType.ProjectPath)]
	public Shader LutBuilderHdrPS;

	[ResourcePath("Shaders/PostProcessing/StopNaN.shader", SearchType.ProjectPath)]
	public Shader StopNanPS;

	[ResourcePath("Shaders/PostProcessing/SubpixelMorphologicalAntialiasing.shader", SearchType.ProjectPath)]
	public Shader SubpixelMorphologicalAntialiasingPS;

	[ResourcePath("Shaders/PostProcessing/TemporalAA.shader", SearchType.ProjectPath)]
	public Shader TemporalAntialiasingPS;

	[ResourcePath("Shaders/PostProcessing/GaussianDepthOfField.shader", SearchType.ProjectPath)]
	public Shader GaussianDepthOfFieldPS;

	[ResourcePath("Shaders/PostProcessing/BokehDepthOfField.shader", SearchType.ProjectPath)]
	public Shader BokehDepthOfFieldPS;

	[ResourcePath("Shaders/PostProcessing/CameraMotionBlur.shader", SearchType.ProjectPath)]
	public Shader CameraMotionBlurPS;

	[ResourcePath("Shaders/PostProcessing/PaniniProjection.shader", SearchType.ProjectPath)]
	public Shader PaniniProjectionPS;

	[ResourcePath("Shaders/PostProcessing/Bloom.shader", SearchType.ProjectPath)]
	public Shader BloomPS;

	[ResourcePath("Shaders/PostProcessing/BloomEnhanced.shader", SearchType.ProjectPath)]
	public Shader BloomEnhancedPS;

	[ResourcePath("Shaders/PostProcessing/RadialBlur.shader", SearchType.ProjectPath)]
	public Shader RadialBlurPS;

	[ResourcePath("Shaders/PostProcessing/MaskedColorTransform.shader", SearchType.ProjectPath)]
	public Shader MaskedColorTransformPS;

	[ResourcePath("Shaders/PostProcessing/UberPost.shader", SearchType.ProjectPath)]
	public Shader UberPostPS;

	[ResourcePath("Shaders/PostProcessing/FinalPost.shader", SearchType.ProjectPath)]
	public Shader FinalPostPassPS;

	[ResourcePath("Shaders/PostProcessing/Daltonization.shader", SearchType.ProjectPath)]
	public Shader DaltonizationPS;

	[ResourcePath("Shaders/Utils/ScreenSpaceCloudShadows.shader", SearchType.ProjectPath)]
	public Shader ScreenSpaceCloudShadowsShader;

	[ResourcePath("Shaders/PostProcessing/LensFlareDataDriven.shader", SearchType.ProjectPath)]
	public Shader LensFlareDataDrivenPS;

	[ResourcePath("Shaders/PostProcessing/LensFlareScreenSpace.shader", SearchType.ProjectPath)]
	public Shader LensFlareScreenSpacePS;

	[ResourcePath("Shaders/PostProcessing/ScalingSetup.shader", SearchType.ProjectPath)]
	public Shader ScalingSetupPS;

	[ResourcePath("Shaders/PostProcessing/EdgeAdaptiveSpatialUpsampling.shader", SearchType.ProjectPath)]
	public Shader EasuPS;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
}
