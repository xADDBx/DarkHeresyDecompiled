using System.Collections.Generic;
using Owlcat.Runtime.Visual.IndirectRendering;
using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class GBufferPass : DrawMultiRendererListPass<GBufferPassData>
{
	private string m_Name;

	private RendererList m_SecondRendererList;

	private GBufferType m_Type;

	public override string Name => m_Name;

	public GBufferPass(RenderPassEvent evt, GBufferType gBufferType)
		: base(evt)
	{
		m_Name = string.Format("{0}.{1}", "GBufferPass", gBufferType);
		m_Type = gBufferType;
	}

	protected override void GetOrCreateRendererLists(ScriptableRenderContext context, ContextContainer frameData, List<DrawMultiRendererListPassData.RendererListData> rendererLists)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		GBufferType type = m_Type;
		if (type != 0 && type == GBufferType.OpaqueDistortion)
		{
			rendererLists.Add(new DrawMultiRendererListPassData.RendererListData(in waaaghRendererListData.OpaqueDistortionGBuffer, WaaaghProfileId.GBuffer_OpaqueDistortion));
			return;
		}
		rendererLists.Add(new DrawMultiRendererListPassData.RendererListData(in waaaghRendererListData.OpaqueGBuffer, WaaaghProfileId.GBuffer_OpaqueBase));
		rendererLists.Add(new DrawMultiRendererListPassData.RendererListData(in waaaghRendererListData.OpaqueAlphaTestGBuffer, WaaaghProfileId.GBuffer_OpaqueAlphaTest));
	}

	protected override void Setup(RenderGraphBuilder builder, GBufferPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthBuffer = builder.UseDepthBuffer(in input, DepthAccess.ReadWrite);
		data.CameraAlbedoRT = builder.UseColorBuffer(in waaaghResourceData.CameraAlbedoRT, 0);
		data.CameraSpecularRT = builder.UseColorBuffer(in waaaghResourceData.CameraSpecularRT, 1);
		data.CameraNormalsRT = builder.UseColorBuffer(in waaaghResourceData.CameraNormalsRT, 2);
		input = waaaghResourceData.CameraColorBuffer;
		data.CameraEmissionRT = builder.UseColorBuffer(in input, 3);
		data.CameraTranslucencyRT = builder.UseColorBuffer(in waaaghResourceData.CameraTranslucencyRT, 4);
		data.CameraBakedGIRT = builder.UseColorBuffer(in waaaghResourceData.CameraBakedGIRT, 5);
		data.CameraShadowmaskRT = builder.UseColorBuffer(in waaaghResourceData.CameraShadowmaskRT, 6);
		input = waaaghResourceData.VTFeedbackRT;
		data.VTFeedbackRT = builder.WriteTexture(in input);
		data.CameraType = waaaghCameraData.cameraType;
		data.IsIndirectRenderingEnabled = waaaghCameraData.IrsData.Enabled;
		data.IsSceneViewInPrefabEditMode = waaaghCameraData.IsSceneViewInPrefabEditMode;
		if (m_Type == GBufferType.Opaque)
		{
			builder.AllowRendererListCulling(!waaaghCameraData.IrsData.IrsHasOpaques);
		}
		else
		{
			builder.AllowRendererListCulling(!waaaghCameraData.IrsData.IrsHasOpaqueDistortions);
		}
	}

	protected override void Render(GBufferPassData data, RenderGraphContext context)
	{
		VirtualTextureManager.SetFeedbackBufferRandomWriteTarget(context.cmd, data.VTFeedbackRT);
		foreach (DrawMultiRendererListPassData.RendererListData rendererList in data.RendererLists)
		{
			using (new ProfilingScope(context.cmd, ProfilingSamplerStorage<WaaaghProfileId>.Get(Name, rendererList.ProfileId)))
			{
				context.cmd.DrawRendererList(rendererList.List);
				IndirectRenderingSystem.Instance.DrawPass(context.cmd, data.CameraType, data.IsIndirectRenderingEnabled, data.IsSceneViewInPrefabEditMode, rendererList.ListParams, debugOverdraw: false);
			}
		}
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraAlbedoRT, data.CameraAlbedoRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraSpecularRT, data.CameraSpecularRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsRT, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraNormalsTexture, data.CameraNormalsRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraTranslucencyRT, data.CameraTranslucencyRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraBakedGIRT, data.CameraBakedGIRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraShadowmaskRT, data.CameraShadowmaskRT);
		context.cmd.ClearRandomWriteTargets();
	}
}
