using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "Fog Of War Runtime Resources", Order = 1000)]
public class FogOfWarFeatureResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/FogOfWar/Shaders/FogOfWar.shader", SearchType.ProjectPath)]
	public Shader FogOfWarShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/FogOfWar/Shaders/ScreenSpaceFogOfWar.shader", SearchType.ProjectPath)]
	public Shader ScreenSpaceFogOfWarShader;

	[SerializeField]
	[ResourcePath("Shaders/Utils/MobileBlur.shader", SearchType.ProjectPath)]
	public Shader BlurShader;

	[SerializeField]
	[ResourcePath("Shaders/Utils/Blit.shader", SearchType.ProjectPath)]
	public Shader BlitShader;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
}
