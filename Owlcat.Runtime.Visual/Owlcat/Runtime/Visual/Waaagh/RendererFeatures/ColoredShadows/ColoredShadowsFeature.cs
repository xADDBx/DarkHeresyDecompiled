using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ColoredShadows.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.ColoredShadows;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Colored Shadows")]
public class ColoredShadowsFeature : ScriptableRendererFeature
{
	[SerializeField]
	private ColoredShadowsSettings m_Settings = new ColoredShadowsSettings();

	private ColoredShadowsCleanupPass m_CleanupPass;

	private ColoredShadowsSetupPass m_SetupPass;

	public override void Create()
	{
		m_SetupPass = new ColoredShadowsSetupPass(RenderPassEvent.BeforeRendering, m_Settings);
		m_CleanupPass = new ColoredShadowsCleanupPass(RenderPassEvent.AfterRendering);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		if (ColoredShadowsSettingsOverride.Resolve(m_Settings).Enable)
		{
			renderer.EnqueuePass(m_SetupPass);
			renderer.EnqueuePass(m_CleanupPass);
		}
	}

	[CanBeNull]
	public static ColoredShadowsFeature GetOrDefault()
	{
		if (!(GraphicsSettings.defaultRenderPipeline is WaaaghPipelineAsset waaaghPipelineAsset))
		{
			return null;
		}
		if (waaaghPipelineAsset.m_RendererDataList == null)
		{
			return null;
		}
		ScriptableRendererData[] rendererDataList = waaaghPipelineAsset.m_RendererDataList;
		foreach (ScriptableRendererData scriptableRendererData in rendererDataList)
		{
			if (scriptableRendererData.RendererFeatures == null)
			{
				continue;
			}
			foreach (ScriptableRendererFeature rendererFeature in scriptableRendererData.RendererFeatures)
			{
				if (rendererFeature is ColoredShadowsFeature result)
				{
					return result;
				}
			}
		}
		return null;
	}
}
