using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.VirtualTexture;

public class VirtualTextureShowPhysicalAtlasPass : ScriptableRenderPass<VirtualTextureShowPhysicalAtlasPassData>
{
	private static class ShaderPropertyId
	{
		public static readonly int _Debug_VTPhysicalAtlas = Shader.PropertyToID("_Debug_VTPhysicalAtlas");

		public static readonly int _Debug_VTPhysicalAtlasArraySlices = Shader.PropertyToID("_Debug_VTPhysicalAtlasArraySlices");

		public static readonly int _Debug_AtlasScaleOffset = Shader.PropertyToID("_Debug_AtlasScaleOffset");

		public static readonly int _Debug_ShowSliceGrid = Shader.PropertyToID("_Debug_ShowSliceGrid");
	}

	private WaaaghDebugData m_DebugData;

	private Material m_Material;

	private int m_Pass;

	public override string Name => "VirtualTextureShowPhysicalAtlasPass";

	public VirtualTextureShowPhysicalAtlasPass(RenderPassEvent evt, WaaaghDebugData debugData, Material material)
		: base(evt)
	{
		m_DebugData = debugData;
		m_Material = material;
		m_Pass = m_Material.FindPass("PhysicalAtlasDebug");
	}

	protected override void Setup(RenderGraphBuilder builder, VirtualTextureShowPhysicalAtlasPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		data.Material = m_Material;
		data.Pass = m_Pass;
		data.VirtualTextureManager = waaaghRenderingData.VirtualTextureManager;
		TextureHandle input = waaaghResourceData.CameraResolveColorBuffer;
		data.CameraFinalTarget = builder.WriteTexture(in input);
		data.ShowPhysicalAtlas = m_DebugData.VirtualTextureDebug.ShowPhysicalAtlas;
		data.ShowSliceGrid = m_DebugData.VirtualTextureDebug.ShowPhysicalAtlasSliceGrid;
		data.Scale = m_DebugData.VirtualTextureDebug.PhyscalAtlasScale;
		data.PhysicalAtlasTex = waaaghRenderingData.VirtualTextureManager.CacheTex;
		data.ScreenSize = new float2(waaaghCameraData.cameraTargetDescriptor.width, waaaghCameraData.cameraTargetDescriptor.height);
		data.FlipVertically = waaaghCameraData.resolveToScreen;
	}

	protected override void Render(VirtualTextureShowPhysicalAtlasPassData data, RenderGraphContext context)
	{
		if (data.ShowPhysicalAtlas)
		{
			float2 screenSize = data.ScreenSize;
			float2 @float = (float2)(math.min(screenSize.x, screenSize.y) * data.Scale) / screenSize;
			float2 float2 = new float2(0f - (1f - @float.x), 0f - (1f - @float.y));
			Vector4 value = new Vector4(@float.x, @float.y, float2.x, data.FlipVertically ? (0f - float2.y) : float2.y);
			context.cmd.SetGlobalTexture(ShaderPropertyId._Debug_VTPhysicalAtlas, data.PhysicalAtlasTex);
			context.cmd.SetGlobalInteger(ShaderPropertyId._Debug_VTPhysicalAtlasArraySlices, data.PhysicalAtlasTex.depth);
			context.cmd.SetGlobalVector(ShaderPropertyId._Debug_AtlasScaleOffset, value);
			context.cmd.SetGlobalInteger(ShaderPropertyId._Debug_ShowSliceGrid, data.ShowSliceGrid ? 1 : 0);
			context.cmd.Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, m_Pass);
		}
	}
}
