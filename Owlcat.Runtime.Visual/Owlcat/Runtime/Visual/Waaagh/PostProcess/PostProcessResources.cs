using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.PostProcess;

public class PostProcessResources
{
	private PostProcessRuntimeShaders m_Shaders;

	private RenderRuntimeTextures m_Textures;

	public PostProcessRuntimeShaders Shaders => m_Shaders;

	public RenderRuntimeTextures Textures => m_Textures;

	public PostProcessResources()
	{
		m_Shaders = GraphicsSettings.GetRenderPipelineSettings<PostProcessRuntimeShaders>();
		m_Textures = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeTextures>();
	}
}
