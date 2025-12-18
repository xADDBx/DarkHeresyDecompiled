using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureFeedbackDebugPass : ScriptableRenderPass<VirtualTextureFeedbackDebugPassData>
{
	private static class ShaderPropertyId
	{
		public static readonly int _Debug_VTFeedback = Shader.PropertyToID("_Debug_VTFeedback");

		public static readonly int _Debug_AtlasScaleOffset = Shader.PropertyToID("_Debug_AtlasScaleOffset");

		public static readonly int _VirtualAtlasSize = Shader.PropertyToID("_VirtualAtlasSize");
	}

	private WaaaghDebugData m_DebugData;

	private Material m_Material;

	private int m_DecodeFeedbackPass;

	private int m_ShowFeedbackTexturePass;

	public override string Name => "VirtualTextureFeedbackDebugPass";

	public VirtualTextureFeedbackDebugPass(RenderPassEvent evt, WaaaghDebugData debugData, Material material)
		: base(evt)
	{
		m_DebugData = debugData;
		m_Material = material;
		m_ShowFeedbackTexturePass = material.FindPass("FeedbackDebug");
		m_DecodeFeedbackPass = material.FindPass("DecodeFeedback");
	}

	protected override void Setup(RenderGraphBuilder builder, VirtualTextureFeedbackDebugPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		data.Material = m_Material;
		data.ShowFeedbackPass = m_ShowFeedbackTexturePass;
		data.DecodeFeedbackPass = m_DecodeFeedbackPass;
		data.VirtualTextureManager = waaaghRenderingData.VirtualTextureManager;
		data.ShowFeedback = m_DebugData.VirtualTextureDebug.ShowFeedback;
		TextureHandle input;
		if (data.ShowFeedback)
		{
			TextureDesc desc = new TextureDesc(data.VirtualTextureManager.VirtualAtlasResolutionInTiles.x, data.VirtualTextureManager.VirtualAtlasResolutionInTiles.y);
			desc.useMipMap = false;
			desc.clearBuffer = false;
			desc.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
			desc.name = "VTFeedbackDebug";
			data.FeedbackDebugTexture = builder.CreateTransientTexture(in desc);
			input = waaaghResourceData.VTFeedbackRT;
			data.FeedbackRT = builder.ReadTexture(in input);
			data.FeedbackDebugTextureSize = new float2(data.VirtualTextureManager.VirtualAtlasResolutionInTiles.x, data.VirtualTextureManager.VirtualAtlasResolutionInTiles.y);
		}
		input = waaaghResourceData.CameraResolveColorBuffer;
		data.CameraFinalTarget = builder.WriteTexture(in input);
		data.ScreenSize = new float2(waaaghCameraData.cameraTargetDescriptor.width, waaaghCameraData.cameraTargetDescriptor.height);
		data.Scale = m_DebugData.VirtualTextureDebug.FeedbackScale;
		data.FlipVertically = waaaghCameraData.resolveToScreen;
	}

	protected override void Render(VirtualTextureFeedbackDebugPassData data, RenderGraphContext context)
	{
		if (data.ShowFeedback)
		{
			context.cmd.SetGlobalVector(ShaderPropertyId._VirtualAtlasSize, new Vector4(data.FeedbackDebugTextureSize.x, data.FeedbackDebugTextureSize.y, 0f, 0f));
			context.cmd.SetGlobalTexture(Owlcat.Runtime.Visual.VirtualTexture.ShaderPropertyId._VTFeedbackBuffer, data.FeedbackRT);
			context.cmd.Blit(context.defaultResources.whiteTexture, data.FeedbackDebugTexture, data.Material, data.DecodeFeedbackPass);
			float2 screenSize = data.ScreenSize;
			float2 @float = data.FeedbackDebugTextureSize * data.Scale / screenSize;
			float2 float2 = new float2(1f - @float.x, 0f - (1f - @float.y));
			Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_VTFeedback, data.FeedbackDebugTexture);
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
			context.cmd.Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, data.ShowFeedbackPass);
		}
	}
}
