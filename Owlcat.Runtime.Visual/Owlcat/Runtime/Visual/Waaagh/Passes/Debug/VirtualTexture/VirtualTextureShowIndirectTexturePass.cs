using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureShowIndirectTexturePass : ScriptableRenderPass<VirtualTextureShowIndirectTexturePassData>
{
	private static class ShaderPropertyId
	{
		public static readonly int _Debug_VTIndirectTexture = Shader.PropertyToID("_Debug_VTIndirectTexture");

		public static readonly int _Debug_AtlasScaleOffset = Shader.PropertyToID("_Debug_AtlasScaleOffset");
	}

	private Material m_Material;

	private int m_Pass;

	private WaaaghDebugData m_DebugData;

	public override string Name => "VirtualTextureShowIndirectTexturePass";

	public VirtualTextureShowIndirectTexturePass(RenderPassEvent evt, WaaaghDebugData debugData, Material material)
		: base(evt)
	{
		m_Material = material;
		m_DebugData = debugData;
		m_Pass = material.FindPass("IndirectTextureDebug");
	}

	protected override void Setup(RenderGraphBuilder builder, VirtualTextureShowIndirectTexturePassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		data.Material = m_Material;
		data.Pass = m_Pass;
		data.VirtualTextureManager = waaaghRenderingData.VirtualTextureManager;
		TextureHandle input = waaaghResourceData.CameraResolveColorBuffer;
		data.CameraFinalTarget = builder.WriteTexture(in input);
		data.ShowIndirectTexture = m_DebugData.VirtualTextureDebug.ShowIndirectionTexture;
		data.IndirectTexture = waaaghRenderingData.VirtualTextureManager.IndirectTexture;
		data.ScreenSize = new float2(waaaghCameraData.cameraTargetDescriptor.width, waaaghCameraData.cameraTargetDescriptor.height);
		data.Scale = m_DebugData.VirtualTextureDebug.IndirectTextureScale;
		data.FlipVertically = waaaghCameraData.resolveToScreen;
	}

	protected override void Render(VirtualTextureShowIndirectTexturePassData data, RenderGraphContext context)
	{
		if (data.ShowIndirectTexture)
		{
			float2 screenSize = data.ScreenSize;
			float2 @float = new float2(data.IndirectTexture.rt.width, data.IndirectTexture.rt.height) * data.Scale / screenSize;
			float2 float2 = 1f - @float;
			Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_VTIndirectTexture, data.IndirectTexture);
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
			context.cmd.Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, data.Pass);
		}
	}
}
