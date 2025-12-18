using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "VolumetricLighting Runtime Resources", Order = 1000)]
public class VolumetricLightingFeatureResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/LocalVolumetricFogCulling.compute", SearchType.ProjectPath)]
	public ComputeShader LocalVolumetricFogCullingCS;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/DebugLocalVolumetricFog.shader", SearchType.ProjectPath)]
	public Shader DebugLocalVolumetricFogPS;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricShadowmapDownsample.shader", SearchType.ProjectPath)]
	public Shader ShadowmapDownsampleShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricSceneVoxelization.compute", SearchType.ProjectPath)]
	public ComputeShader VoxelizationShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricLighting.compute", SearchType.ProjectPath)]
	public ComputeShader LightingShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricScatter.compute", SearchType.ProjectPath)]
	public ComputeShader ScatterShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/VolumetricLighting/Shaders/VolumetricApplyOpaque.shader", SearchType.ProjectPath)]
	public Shader ApplyOpaqueShader;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
}
