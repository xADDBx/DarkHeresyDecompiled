using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh;

public enum WaaaghProfileId
{
	Prepare,
	PreRender,
	PostRender,
	PostRenderSubmit,
	[HideInDebugUI]
	PipelineBeginContextRendering,
	[HideInDebugUI]
	PipelineEndContextRendering,
	[HideInDebugUI]
	PipelineBeginCameraRendering,
	[HideInDebugUI]
	PipelineEndCameraRendering,
	CameraCull,
	CameraSetupData,
	CameraSetupRenderer,
	CameraExecuteRenderer,
	CameraSubmit,
	RendererSortRenderPasses,
	RendererRecordRenderGraph,
	RendererCreateResources,
	RendererRecordPasses,
	RendererExecuteRenderGraph,
	RenderGraphBeginRecording,
	RendererSetupBasePasses,
	RendererSetup,
	RendererConfigureRendererLists,
	RendererInitSharedRendererLists,
	RendererConfigurePassesRendererLists,
	RendererEnqueuePasses,
	RendererSetupVFX,
	RendererPrepareRendererListsAsync,
	BuildCameraStack,
	RenderCameraStack,
	PrepareCamera,
	RenderCamera,
	UpdateVolumeFramework,
	[HideInDebugUI]
	RenderInitializeAdditionalCameraData,
	[HideInDebugUI]
	RenderInitializeCameraData,
	[HideInDebugUI]
	RenderInitializeStackedCameraData,
	[WaaaghProfileCategory(WaaaghProfileCategory.Lighting)]
	RendererSetupLights,
	RendererSetupLightsComplete,
	RendererSetupFeatures,
	RendererSetupFeaturesComplete,
	[WaaaghProfileCategory(WaaaghProfileCategory.Lighting)]
	LightCookieSetup,
	[WaaaghProfileCategory(WaaaghProfileCategory.Shadows)]
	ShadowsSetup,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	[DisplayInfo(name = "GPUDrivenCullingPass.Prepare")]
	GPUDrivenCullingPass_Prepare,
	[WaaaghProfileCategory(WaaaghProfileCategory.Shadows)]
	[DisplayInfo(name = "GPUDrivenCullingPass.Shadows")]
	GPUDrivenCullingPass_Shadows,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	[DisplayInfo(name = "GPUDrivenCullingPass")]
	GPUDrivenCullingPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	[DisplayInfo(name = "GPUDrivenCullingPass.First")]
	GPUDrivenCullingPass_First,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	[DisplayInfo(name = "GPUDrivenCullingPass.FalseNegative")]
	GPUDrivenCullingPass_FalseNegative,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	[DisplayInfo(name = "GPUDrivenCullingPass.HDB")]
	GPUDrivenCullingPass_HDB,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	[DisplayInfo(name = "DepthPrePass.OpaqueBase")]
	DepthPrePass_OpaqueBase,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueAlphaTest)]
	[DisplayInfo(name = "DepthPrePass.OpaqueAlphaTest")]
	DepthPrePass_OpaqueAlphaTest,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueDistortion)]
	[DisplayInfo(name = "DepthPrePass.OpaqueDistortion")]
	DepthPrePass_OpaqueDistortion,
	[WaaaghProfileCategory(WaaaghProfileCategory.Shadows)]
	NativeShadowCasterPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	[DisplayInfo(name = "GBufferPass.OpaqueBase")]
	GBuffer_OpaqueBase,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueAlphaTest)]
	[DisplayInfo(name = "GBufferPass.OpaqueAlphaTest")]
	GBuffer_OpaqueAlphaTest,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueDistortion)]
	[DisplayInfo(name = "GBufferPass.OpaqueDistortion")]
	GBuffer_OpaqueDistortion,
	[WaaaghProfileCategory(WaaaghProfileCategory.Terrain)]
	TerrainPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Decals)]
	DrawDecalsPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	MotionVectorsPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueBase)]
	MotionVectors,
	[WaaaghProfileCategory(WaaaghProfileCategory.Lighting)]
	LightCullingPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Lighting)]
	DeferredLightingPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Lighting)]
	DeferredLightingBuildVariantsPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Lighting)]
	DrawSkyboxPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Reflections)]
	DepthPyramidPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Reflections)]
	DeferredReflectionsPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Reflections)]
	GPUDrivenForwardReflectionProbesPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueDistortion)]
	[DisplayInfo(name = "DrawObjectsPass.OpaqueDistortionForward")]
	DrawObjects_OpaqueDistortionForward,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_Transparent)]
	[DisplayInfo(name = "DrawObjectsPass.Transparent")]
	DrawObjects_Transparent,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_Transparent)]
	[DisplayInfo(name = "DrawObjectsPass.Overlay")]
	DrawObjects_Overlay,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_OpaqueDistortion)]
	[DisplayInfo(name = "DrawColorPyramidPass.OpaqueDistortion")]
	DrawColorPyramidPass_OpaqueDistortion,
	[WaaaghProfileCategory(WaaaghProfileCategory.Geometry_Transparent)]
	[DisplayInfo(name = "DrawColorPyramidPass.TransparentDistortion")]
	DrawColorPyramidPass_TransparentDistortion,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	FogPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.UI)]
	DrawScreenSpaceUIPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	RenderPostProcess,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	RenderPostProcessFinal,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	ColorGradingLUT,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	StopNaNs,
	[WaaaghProfileCategory(WaaaghProfileCategory.AntiAliasing)]
	SMAAMaterialSetup,
	[WaaaghProfileCategory(WaaaghProfileCategory.AntiAliasing)]
	SMAAEdgeDetection,
	[WaaaghProfileCategory(WaaaghProfileCategory.AntiAliasing)]
	SMAABlendWeight,
	[WaaaghProfileCategory(WaaaghProfileCategory.AntiAliasing)]
	SMAANeighborhoodBlend,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	SetupDoF,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	DOFComputeCOC,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	DOFDownscalePrefilter,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	DOFBlurH,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	DOFBlurV,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	DOFComposite,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	DOFBlurBokeh,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	DOFPostFilter,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	BloomSetup,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	Bloom,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	BloomPrefilter,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	BloomDownsample,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	BloomUpsample,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	LensFlareScreenSpace,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	LensFlareDataDrivenComputeOcclusion,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	LensFlareDataDriven,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	MotionBlur,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	PaniniProjection,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	UberPostSetupBloomPass,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	UberPost,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	FinalSetup,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	FinalFSRScale,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	FinalPostBlit,
	[WaaaghProfileCategory(WaaaghProfileCategory.Reflections)]
	SSR,
	[WaaaghProfileCategory(WaaaghProfileCategory.PostProcessing)]
	RenderBeforeTransparentPostProcess,
	[WaaaghProfileCategory(WaaaghProfileCategory.Lighting)]
	SSAO,
	[WaaaghProfileCategory(WaaaghProfileCategory.AntiAliasing)]
	TAA,
	[WaaaghProfileCategory(WaaaghProfileCategory.AntiAliasing)]
	TAACopyHistory
}
