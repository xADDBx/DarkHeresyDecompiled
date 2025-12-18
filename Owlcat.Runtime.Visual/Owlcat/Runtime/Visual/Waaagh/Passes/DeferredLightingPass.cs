using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DeferredLightingPass : ScriptableRenderPass<DeferredLightingPassData>
{
	private readonly Material m_DeferredLightingMaterial;

	public override string Name => "DeferredLightingPass";

	private protected sealed override WaaaghProfileId? ProfileId => WaaaghProfileId.DeferredLightingPass;

	public DeferredLightingPass(RenderPassEvent evt, Material deferredLightingMaterial)
		: base(evt)
	{
		m_DeferredLightingMaterial = deferredLightingMaterial;
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		DependsOn(in waaaghRendererListData.OpaqueGBuffer.List);
		DependsOn(in waaaghRendererListData.OpaqueAlphaTestGBuffer.List);
		DependsOn(in waaaghRendererListData.TerrainGBuffer.List);
	}

	protected override void Setup(RenderGraphBuilder builder, DeferredLightingPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorRT = builder.UseColorBuffer(in input, 0);
		input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthRT = builder.UseDepthBuffer(in input, DepthAccess.Read);
		data.CameraDepthCopytRT = builder.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
		data.CameraAlbedoRT = builder.ReadTexture(in waaaghResourceData.CameraAlbedoRT);
		data.CameraNormalsRT = builder.ReadTexture(in waaaghResourceData.CameraNormalsRT);
		data.CameraBakedGIRT = builder.ReadTexture(in waaaghResourceData.CameraBakedGIRT);
		data.CameraShadowmaskRT = builder.ReadTexture(in waaaghResourceData.CameraShadowmaskRT);
		data.CameraTranslucencyRT = builder.ReadTexture(in waaaghResourceData.CameraTranslucencyRT);
		if (waaaghResourceData.Shadowmap.IsValid())
		{
			input = waaaghResourceData.Shadowmap;
			builder.ReadTexture(in input);
		}
		data.DeferredLightingMaterial = m_DeferredLightingMaterial;
		SphericalHarmonicsL2 ambientProbe = RenderSettings.ambientProbe;
		Color glossyEnvironmentColor = CoreUtils.ConvertLinearToActiveColorSpace(new Color(ambientProbe[0, 0], ambientProbe[1, 0], ambientProbe[2, 0]) * RenderSettings.reflectionIntensity);
		data.GlossyEnvironmentColor = glossyEnvironmentColor;
		data.GlossyBlackColor = default(Color);
	}

	protected override void Render(DeferredLightingPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentColor, data.GlossyBlackColor);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.DeferredLightingMaterial, 0, MeshTopology.Triangles, 3);
		context.cmd.SetGlobalVector(ShaderPropertyId._GlossyEnvironmentColor, data.GlossyEnvironmentColor);
	}
}
