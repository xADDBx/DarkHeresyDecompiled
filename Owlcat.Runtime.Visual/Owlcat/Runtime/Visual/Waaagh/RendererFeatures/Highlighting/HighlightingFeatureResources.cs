using System;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;
using UnityEngine.Categorization;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.Highlighting;

[Serializable]
[SupportedOnRenderPipeline(typeof(WaaaghPipelineAsset))]
[CategoryInfo(Name = "Highlighting Runtime Resources", Order = 1000)]
public class HighlightingFeatureResources : IRenderPipelineResources, IRenderPipelineGraphicsSettings
{
	[SerializeField]
	[HideInInspector]
	private int m_Version;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/Highlighting/Shaders/Highlighter.shader", SearchType.ProjectPath)]
	public Shader HighlighterShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/Highlighting/Shaders/HighlightingBlur.shader", SearchType.ProjectPath)]
	public Shader BlurShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/Highlighting/Shaders/HighlightingCut.shader", SearchType.ProjectPath)]
	public Shader CutShader;

	[SerializeField]
	[ResourcePath("Runtime/Waaagh/RendererFeatures/Highlighting/Shaders/HighlightingComposite.shader", SearchType.ProjectPath)]
	public Shader CompositeShader;

	public int version => m_Version;

	bool IRenderPipelineGraphicsSettings.isAvailableInPlayerBuild => true;
}
