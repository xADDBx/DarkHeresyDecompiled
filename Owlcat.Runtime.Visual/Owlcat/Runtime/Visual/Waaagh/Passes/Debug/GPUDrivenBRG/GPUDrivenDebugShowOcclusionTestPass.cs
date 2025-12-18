using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug.GPUDrivenBRG;

public class GPUDrivenDebugShowOcclusionTestPass : ScriptableRenderPass<GPUDrivenDebugShowOcclusionTestPassData>
{
	private static class ShaderIDs
	{
		public static int _OcclusionTestCountRange = Shader.PropertyToID("_OcclusionTestCountRange");

		public static int _OcclusionTestOpacity = Shader.PropertyToID("_OcclusionTestOpacity");
	}

	private readonly WaaaghDebugData m_DebugData;

	private readonly Material m_Material;

	private readonly int m_Pass;

	public override string Name => "GPUDrivenDebug.ShowOcclusionTest";

	public GPUDrivenDebugShowOcclusionTestPass(RenderPassEvent evt, Material material, WaaaghDebugData debugData)
		: base(evt)
	{
		m_Material = material;
		m_Pass = material.FindPass("ShowOcclusionTest");
		m_DebugData = debugData;
	}

	protected override void Setup(RenderGraphBuilder builder, GPUDrivenDebugShowOcclusionTestPassData data, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		data.Material = m_Material;
		data.Pass = m_Pass;
		TextureHandle input = waaaghResourceData.CameraResolveColorBuffer;
		data.CameraFinalTarget = builder.ReadWriteTexture(in input);
		builder.ReadBuffer(in waaaghRenderingData.GPUDrivenBatchRendererGroup.SharedPassData.Buffers.MainViewOcclusionTestDebug);
		data.Material.SetFloat(ShaderIDs._OcclusionTestCountRange, math.max(1, m_DebugData.GPUDrivenBRGDebug.OcclusionTestCountRange));
		data.Material.SetFloat(ShaderIDs._OcclusionTestOpacity, math.saturate(m_DebugData.GPUDrivenBRGDebug.OcclusionTestOpacity));
	}

	protected override void Render(GPUDrivenDebugShowOcclusionTestPassData data, RenderGraphContext context)
	{
		context.cmd.Blit(context.defaultResources.whiteTexture, data.CameraFinalTarget, data.Material, data.Pass);
	}
}
