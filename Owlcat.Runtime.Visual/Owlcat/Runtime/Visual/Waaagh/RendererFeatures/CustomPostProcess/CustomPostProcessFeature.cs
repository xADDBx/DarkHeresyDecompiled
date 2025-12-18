using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.CustomPostProcess;
using Owlcat.Runtime.Visual.Overrides.CustomPostProcess;
using Owlcat.Runtime.Visual.Utilities;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess.Passes;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess;

[CreateAssetMenu(menuName = "Renderer Features/Waaagh/Custom Post Process/Feature", fileName = "CustomPostProcessFeature")]
public class CustomPostProcessFeature : ScriptableRendererFeature
{
	private List<CustomPostProcessPass> m_Passes;

	private StencilMaskPass m_StencilMaskPass;

	private Material m_StencilMaskMaterial;

	public List<CustomPostProcessEffect> Effects;

	public StencilMaskTextureSettings StencilMaskSettings = new StencilMaskTextureSettings
	{
		Ref = StencilRef.ShaderGraphOverride,
		ReadMask = StencilRef.ShaderGraphOverride,
		CompareFunction = CompareFunction.Equal,
		FilterMode = FilterMode.Bilinear
	};

	public override void Create()
	{
		m_Passes = new List<CustomPostProcessPass>();
		foreach (IGrouping<CustomPostProcessRenderEvent, CustomPostProcessEffect> item in from e in Effects.Where((CustomPostProcessEffect e) => e).Distinct()
			group e by e.Event)
		{
			List<CustomPostProcessEffect> effects = item.Where((CustomPostProcessEffect e) => e).ToList();
			m_Passes.Add(new CustomPostProcessPass(ConvertEvent(item.Key), effects));
		}
		RenderRuntimeShaders renderPipelineSettings = GraphicsSettings.GetRenderPipelineSettings<RenderRuntimeShaders>();
		m_StencilMaskMaterial = CoreUtils.CreateEngineMaterial(renderPipelineSettings.StencilMaskPS);
		m_StencilMaskPass = new StencilMaskPass(m_StencilMaskMaterial, RenderPassEvent.BeforeRenderingPostProcessing);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		Validate(waaaghCameraData.camera);
		bool postProcessEnabled = waaaghCameraData.postProcessEnabled;
		Owlcat.Runtime.Visual.Overrides.CustomPostProcess.CustomPostProcess component = VolumeManager.instance.stack.GetComponent<Owlcat.Runtime.Visual.Overrides.CustomPostProcess.CustomPostProcess>();
		if (!postProcessEnabled || !component.IsActive())
		{
			return;
		}
		if (IsStencilRequired(component))
		{
			m_StencilMaskPass.ConfigureSettings(in StencilMaskSettings);
			renderer.EnqueuePass(m_StencilMaskPass);
		}
		foreach (CustomPostProcessPass pass in m_Passes)
		{
			renderer.EnqueuePass(pass);
		}
	}

	private bool IsStencilRequired(Owlcat.Runtime.Visual.Overrides.CustomPostProcess.CustomPostProcess customPostProcessSettings)
	{
		foreach (CustomPostProcessPass pass in m_Passes)
		{
			foreach (CustomPostProcessEffect effect in pass.Effects)
			{
				if (effect.UseStencilMask && customPostProcessSettings.IsEffectActive(effect))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Validate(Camera camera)
	{
		bool flag = false;
		for (int i = 0; i < m_Passes.Count; i++)
		{
			CustomPostProcessPass customPostProcessPass = m_Passes[i];
			RenderPassEvent renderPassEvent = customPostProcessPass.RenderPassEvent;
			customPostProcessPass.Validate();
			for (int j = 0; j < customPostProcessPass.Effects.Count; j++)
			{
				CustomPostProcessEffect customPostProcessEffect = customPostProcessPass.Effects[j];
				if (ConvertEvent(customPostProcessEffect.Event) != renderPassEvent)
				{
					flag = true;
					break;
				}
				foreach (CustomPostProcessEffectPass pass in customPostProcessEffect.Passes)
				{
					if (pass.Shader != null && !customPostProcessPass.ValidateMaterial(camera, pass))
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			Dispose();
			Create();
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		foreach (CustomPostProcessPass pass in m_Passes)
		{
			pass.Cleanup();
		}
		CoreUtils.Destroy(m_StencilMaskMaterial);
	}

	public RenderPassEvent ConvertEvent(CustomPostProcessRenderEvent evt)
	{
		return evt switch
		{
			CustomPostProcessRenderEvent.BeforeMainPostProcess => RenderPassEvent.BeforeRenderingPostProcessing, 
			CustomPostProcessRenderEvent.AfterMainPostProcess => RenderPassEvent.AfterRenderingPostProcessing, 
			_ => RenderPassEvent.AfterRenderingPostProcessing, 
		};
	}

	public CustomPostProcessRenderEvent ConvertEvent(RenderPassEvent evt)
	{
		return evt switch
		{
			RenderPassEvent.BeforeRenderingPostProcessing => CustomPostProcessRenderEvent.BeforeMainPostProcess, 
			RenderPassEvent.AfterRenderingPostProcessing => CustomPostProcessRenderEvent.AfterMainPostProcess, 
			_ => CustomPostProcessRenderEvent.AfterMainPostProcess, 
		};
	}
}
