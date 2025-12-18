using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class VolumetricLightingApplyOpaquePass : ScriptableRenderPass<VolumetricLightingApplyOpaquePassData>
{
	private Material m_Material;

	private static int _CameraColor = Shader.PropertyToID("_CameraColor");

	public override string Name => "VolumetricLightingApplyOpaquePass";

	public VolumetricLightingApplyOpaquePass(RenderPassEvent evt, Material material)
		: base(evt)
	{
		m_Material = material;
	}

	protected override void Setup(RenderGraphBuilder builder, VolumetricLightingApplyOpaquePassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		data.Material = m_Material;
		data.ScatterTexture = builder.ReadTexture(in waaaghResourceData.VolumetricScatter);
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorBuffer = builder.UseColorBuffer(in input, 0);
		data.DepthCopyTexture = builder.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
	}

	protected override void Render(VolumetricLightingApplyOpaquePassData data, RenderGraphContext context)
	{
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}
}
