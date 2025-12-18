using Owlcat.Runtime.Visual.FogOfWar;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.FogOfWar.Passes;

public class FogOfWarPostProcessPass : ScriptableRenderPass<FogOfWarPostProcessPassData>
{
	private Material m_FowMaterial;

	private int m_ShaderPass;

	private FogOfWarArea m_Area;

	private FogOfWarFeature m_Feature;

	private FogOfWarSettings m_Settings;

	public override string Name => "FogOfWarPostProcessPass";

	public FogOfWarPostProcessPass(RenderPassEvent evt, Material fowMaterial)
		: base(evt)
	{
		m_FowMaterial = fowMaterial;
		m_ShaderPass = m_FowMaterial.FindPass("DRAW FOW SCREEN SPACE");
	}

	internal void Init(FogOfWarArea activeArea, FogOfWarFeature fogOfWarFeature, FogOfWarSettings settings)
	{
		m_Area = activeArea;
		m_Feature = fogOfWarFeature;
		m_Settings = settings;
	}

	protected override void Setup(RenderGraphBuilder builder, FogOfWarPostProcessPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		data.FowMaterial = m_FowMaterial;
		data.ShaderPass = m_ShaderPass;
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorRT = builder.WriteTexture(in input);
	}

	protected override void Render(FogOfWarPostProcessPassData data, RenderGraphContext context)
	{
		context.cmd.Blit(BuiltinRenderTextureType.None, data.CameraColorRT, data.FowMaterial, data.ShaderPass);
	}
}
