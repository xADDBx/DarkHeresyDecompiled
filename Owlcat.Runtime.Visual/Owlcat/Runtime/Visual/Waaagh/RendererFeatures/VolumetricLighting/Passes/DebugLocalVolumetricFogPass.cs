using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

public class DebugLocalVolumetricFogPass : ScriptableRenderPass<DebugLocalVolumetricFogPassData>
{
	private Material m_DebugMaterial;

	private VolumetricLightingFeature m_Feature;

	public override string Name => "DebugLocalVolumetricFogPass";

	public DebugLocalVolumetricFogPass(RenderPassEvent evt, VolumetricLightingFeature feature, Material debugMaterial)
		: base(evt)
	{
		m_DebugMaterial = debugMaterial;
		m_Feature = feature;
	}

	protected override void Setup(RenderGraphBuilder builder, DebugLocalVolumetricFogPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		data.Material = m_DebugMaterial;
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorBuffer = builder.UseColorBuffer(in input, 0);
		data.DepthCopyTexture = builder.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
		data.LocalFogClusteringParams = m_Feature.FogClusteringParams;
		data.FogTilesBuffer = builder.ReadBuffer(in m_Feature.FogTilesBufferHandle);
		data.FogZBinsBuffer = builder.ReadBuffer(in m_Feature.ZBinsBufferHandle);
	}

	protected override void Render(DebugLocalVolumetricFogPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalVector(ShaderPropertyId._LocalVolumetricFogClusteringParams, data.LocalFogClusteringParams);
		context.cmd.SetGlobalBuffer(ShaderPropertyId._FogTilesBuffer, data.FogTilesBuffer);
		context.cmd.SetGlobalBuffer(ShaderPropertyId._LocalFogZBinsBuffer, data.FogZBinsBuffer);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}
}
