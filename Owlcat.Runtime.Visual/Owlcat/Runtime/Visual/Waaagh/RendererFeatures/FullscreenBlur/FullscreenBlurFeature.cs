using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FullscreenBlur.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FullscreenBlur;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Fullscreen Blur")]
public class FullscreenBlurFeature : ScriptableRendererFeature
{
	private FullscreenBlurFeatureResources m_Resources;

	public RenderPassEvent Event;

	public int EventOffset;

	public BlurType BlurType;

	public Downsample Downsample = Downsample.Downsample2x2;

	[Range(0f, 10f)]
	public float BlurSize;

	[Range(1f, 4f)]
	public int BlurIterations = 1;

	private FullscreenBlurPass m_Pass;

	private Material m_BlurMaterial;

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		if (BlurSize > 0f)
		{
			renderer.EnqueuePass(m_Pass);
		}
	}

	public override void Create()
	{
		m_Resources = GraphicsSettings.GetRenderPipelineSettings<FullscreenBlurFeatureResources>();
		m_BlurMaterial = CoreUtils.CreateEngineMaterial(m_Resources.BlurShader);
		m_Pass = new FullscreenBlurPass(Event + EventOffset, this, m_BlurMaterial);
	}

	protected override void Dispose(bool disposing)
	{
		CoreUtils.Destroy(m_BlurMaterial);
	}
}
