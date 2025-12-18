using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.OccludedObjectHighlighting;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "OccludedObjectHighlighting Runtime Resources", Order = 1000)]
public class OccludedObjectHighlightingFeatureResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/OccludedObjectHighlighting/Shaders/OccludedObjectHighlighter.shader", SearchType.ProjectPath)]
	public Shader HighlighterShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/Highlighting/Shaders/HighlightingBlur.shader", SearchType.ProjectPath)]
	public Shader BlurShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/OccludedObjectHighlighting/Shaders/OccludedObjectHighlightingComposite.shader", SearchType.ProjectPath)]
	public Shader CompositeShader;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
}
