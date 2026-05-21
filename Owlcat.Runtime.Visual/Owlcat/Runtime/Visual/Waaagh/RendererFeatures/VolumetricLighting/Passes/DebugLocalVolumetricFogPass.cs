using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.VolumetricLighting.Passes;

internal static class DebugLocalVolumetricFogPass
{
	private sealed class PassData
	{
		public Material DebugMaterial;

		public Vector4 LocalFogClusteringParams;

		public BufferHandle FogTilesBuffer;

		public BufferHandle FogZBinsBuffer;
	}

	public static void Record(in RecordContext context, VolumetricLightingRendererFeature feature, Material debugMaterial, VolumetricLightingData volumetricLightingData)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("Draw Local Volumetric Fog Debug", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\RendererFeatures\\VolumetricLighting\\Passes\\DebugLocalVolumetricFogPass.cs", 22);
		passData.DebugMaterial = debugMaterial;
		passData.LocalFogClusteringParams = feature.FogClusteringParams;
		passData.FogTilesBuffer = volumetricLightingData.FogTilesBuffer;
		passData.FogZBinsBuffer = volumetricLightingData.ZBinsBuffer;
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthRT);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthTexture);
		unsafeRenderGraphBuilder.UseBuffer(in volumetricLightingData.FogTilesBuffer);
		unsafeRenderGraphBuilder.UseBuffer(in volumetricLightingData.ZBinsBuffer);
		unsafeRenderGraphBuilder.SetRenderAttachment(context.FrameResources.CameraStackTargets.Color, 0);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			context.cmd.SetGlobalVector(ShaderPropertyId._LocalVolumetricFogClusteringParams, data.LocalFogClusteringParams);
			context.cmd.SetGlobalBuffer(ShaderPropertyId._FogTilesBuffer, data.FogTilesBuffer);
			context.cmd.SetGlobalBuffer(ShaderPropertyId._LocalFogZBinsBuffer, data.FogZBinsBuffer);
			context.cmd.DrawProcedural(Matrix4x4.identity, data.DebugMaterial, 0, MeshTopology.Triangles, 3);
		});
	}
}
