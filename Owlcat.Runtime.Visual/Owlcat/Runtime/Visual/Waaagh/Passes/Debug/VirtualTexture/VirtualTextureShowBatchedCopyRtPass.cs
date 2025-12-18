using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureShowBatchedCopyRtPass : ScriptableRenderPass<VirtualTextureShowBatchedCopyRtPassData>
{
	private static class ShaderPropertyId
	{
		public static readonly int _Debug_VTBatchedCopyRt = Shader.PropertyToID("_Debug_VTBatchedCopyRt");

		public static readonly int _Debug_AtlasScaleOffset = Shader.PropertyToID("_Debug_AtlasScaleOffset");

		public static readonly int m_Debug_MipLevel = Shader.PropertyToID("m_Debug_MipLevel");
	}

	private WaaaghDebugData m_DebugData;

	private Material m_Material;

	private int m_Pass;

	public override string Name => "VirtualTextureShowBatchedCopyRtPass";

	public VirtualTextureShowBatchedCopyRtPass(RenderPassEvent evt, WaaaghDebugData debugData, Material material)
		: base(evt)
	{
		m_DebugData = debugData;
		m_Material = material;
		m_Pass = m_Material.FindPass("ShowBatchedCopyRt");
	}

	protected override void Setup(RenderGraphBuilder builder, VirtualTextureShowBatchedCopyRtPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		data.Material = m_Material;
		data.Pass = m_Pass;
		data.VirtualTextureManager = waaaghRenderingData.VirtualTextureManager;
		TextureHandle input = waaaghResourceData.CameraResolveColorBuffer;
		data.CameraFinalTarget = builder.WriteTexture(in input);
		data.ShowBatchedCopyRt = m_DebugData.VirtualTextureDebug.ShowBatchedCopyRt;
		if (data.ShowBatchedCopyRt)
		{
			data.BatchedCopyRt = waaaghRenderingData.VirtualTextureManager.TileUploader.BatchedCopyRt;
			data.BatchedCopyDebugTexture = waaaghRenderingData.VirtualTextureManager.TileUploader.BatchedCopyDebugTex;
			data.Scale = m_DebugData.VirtualTextureDebug.BatchedCopyRtScale;
			data.ScreenSize = new float2(waaaghCameraData.cameraTargetDescriptor.width, waaaghCameraData.cameraTargetDescriptor.height);
			data.Limit = (int)waaaghRenderingData.VirtualTextureManager.TileUploader.Limit;
		}
		data.FlipVertically = waaaghCameraData.resolveToScreen;
	}

	protected override void Render(VirtualTextureShowBatchedCopyRtPassData data, RenderGraphContext context)
	{
		if (data.ShowBatchedCopyRt)
		{
			context.cmd.CopyTexture(data.BatchedCopyRt, 0, 0, 0, 0, data.BatchedCopyRt.rt.width, data.BatchedCopyRt.rt.height, data.BatchedCopyDebugTexture, 0, 0, 0, 0);
			float2 screenSize = data.ScreenSize;
			float2 @float = (float2)(math.min(screenSize.x, screenSize.y) * data.Scale) / screenSize;
			float2 float2 = new float2(0f - (1f - @float.x), 1f - @float.y);
			Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_VTBatchedCopyRt, data.BatchedCopyDebugTexture);
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
			context.cmd.SetGlobalFloat(ShaderPropertyId.m_Debug_MipLevel, 0f);
			context.cmd.Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, m_Pass);
		}
	}
}
