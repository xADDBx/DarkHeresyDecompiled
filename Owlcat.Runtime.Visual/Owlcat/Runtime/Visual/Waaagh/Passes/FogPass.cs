using Owlcat.Runtime.Visual.Waaagh.FrameData;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class FogPass : ScriptableRenderPass<FogPassData>
{
	private Material m_Material;

	public override string Name => "FogPass";

	private protected override WaaaghProfileId? ProfileId => WaaaghProfileId.FogPass;

	public FogPass(RenderPassEvent evt, Material material)
		: base(evt)
	{
		m_Material = material;
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		DependsOn(in waaaghRendererListData.OpaqueGBuffer.List);
		DependsOn(in waaaghRendererListData.OpaqueAlphaTestGBuffer.List);
		DependsOn(in waaaghRendererListData.TerrainGBuffer.List);
	}

	protected override void Setup(RenderGraphBuilder builder, FogPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		data.Material = m_Material;
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorRT = builder.UseColorBuffer(in input, 0);
		input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthCopyRT = builder.ReadTexture(in input);
	}

	protected override void Render(FogPassData data, RenderGraphContext context)
	{
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}
}
