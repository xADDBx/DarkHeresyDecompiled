using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Passes;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess.Passes;

public sealed class StencilMaskPass : ScriptableRasterPass<StencilMaskPassData>
{
	private static class ShaderIDs
	{
		public static readonly int _StencilMaskRef = Shader.PropertyToID("_StencilMaskRef");

		public static readonly int _StencilMaskReadMask = Shader.PropertyToID("_StencilMaskReadMask");

		public static readonly int _StencilMaskComp = Shader.PropertyToID("_StencilMaskComp");

		public static readonly int _CameraStencilMask = Shader.PropertyToID("_CameraStencilMask");
	}

	private const int kShaderPass = 0;

	private readonly Material m_Material;

	private StencilMaskTextureSettings m_Settings;

	public override string Name => "StencilMaskPass";

	public StencilMaskPass(Material material, RenderPassEvent evt)
		: base(evt)
	{
		m_Material = material;
	}

	protected override void Render(StencilMaskPassData data, RasterGraphContext context)
	{
		context.cmd.SetGlobalInteger(ShaderIDs._StencilMaskRef, (int)data.Ref);
		context.cmd.SetGlobalInteger(ShaderIDs._StencilMaskReadMask, (int)data.ReadMask);
		context.cmd.SetGlobalInteger(ShaderIDs._StencilMaskComp, (int)data.CompareFunction);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}

	public void ConfigureSettings(in StencilMaskTextureSettings settings)
	{
		m_Settings = settings;
	}

	protected override void Setup(IRasterRenderGraphBuilder builder, StencilMaskPassData passData, ContextContainer frameData)
	{
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghPostProcessingData waaaghPostProcessingData = frameData.Get<WaaaghPostProcessingData>();
		TextureDesc desc = RenderingUtils.CreateTextureDesc("StencilMask", waaaghCameraData.cameraTargetDescriptor);
		desc.clearBuffer = true;
		desc.colorFormat = GraphicsFormat.R8_UNorm;
		desc.clearColor = Color.clear;
		desc.filterMode = m_Settings.FilterMode;
		waaaghPostProcessingData.StencilMaskTexture = waaaghRenderingData.RenderGraph.CreateTexture(in desc);
		builder.SetRenderAttachment(waaaghPostProcessingData.StencilMaskTexture, 0);
		builder.SetRenderAttachmentDepth(waaaghResourceData.CameraDepthBuffer, AccessFlags.Read);
		builder.SetGlobalTextureAfterPass(in waaaghPostProcessingData.StencilMaskTexture, ShaderIDs._CameraStencilMask);
		builder.AllowGlobalStateModification(value: true);
		passData.Ref = m_Settings.Ref;
		passData.ReadMask = m_Settings.ReadMask;
		passData.CompareFunction = m_Settings.CompareFunction;
		passData.Material = m_Material;
	}
}
