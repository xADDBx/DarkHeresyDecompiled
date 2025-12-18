using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.Debugging;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.ShaderLibrary.Visual.Debug;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.Debug;

public sealed class DebugQuadOverdrawPass : ScriptableRenderPass<DebugQuadOverdrawPass.PassData>
{
	public sealed class PassData : PassDataBase
	{
		public readonly List<RendererListHandle> RendererLists = new List<RendererListHandle>();

		public BufferHandle FullScreenDebugBuffer;

		public void Clear()
		{
			RendererLists.Clear();
		}
	}

	private readonly WaaaghDebugData m_DebugData;

	private readonly RenderGraphDebugResources m_Resources;

	private readonly Shader m_Shader;

	public override string Name => "DebugQuadOverdrawPass";

	public DebugQuadOverdrawPass(WaaaghDebugData debugData, RenderGraphDebugResources resources)
		: base(RenderPassEvent.AfterRenderingTransparents)
	{
		m_DebugData = debugData;
		m_Resources = resources;
		m_Shader = m_DebugData.Resources.DebugOverdrawPS;
	}

	protected override void Setup(RenderGraphBuilder builder, PassData passData, ContextContainer frameData)
	{
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		frameData.Get<WaaaghCameraData>();
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		if (m_DebugData.RenderingDebug.OverdrawMode != DebugOverdrawMode.QuadOverdraw || !m_Resources.FullScreenDebugBuffer.IsValid() || m_Shader == null)
		{
			return;
		}
		passData.Clear();
		bool num = m_DebugData.RenderingDebug.QuadOverdrawSettings.ObjectFilter == QuadOverdrawObjectFilter.All || m_DebugData.RenderingDebug.QuadOverdrawSettings.ObjectFilter == QuadOverdrawObjectFilter.OpaqueOnly;
		bool flag = m_DebugData.RenderingDebug.QuadOverdrawSettings.ObjectFilter == QuadOverdrawObjectFilter.All || m_DebugData.RenderingDebug.QuadOverdrawSettings.ObjectFilter == QuadOverdrawObjectFilter.TransparentOnly;
		if (num)
		{
			CreateOverdrawRendererLists(waaaghRenderingData.RenderGraph, waaaghRendererListData.OpaqueGBuffer.ListParams, passData.RendererLists);
			CreateOverdrawRendererLists(waaaghRenderingData.RenderGraph, waaaghRendererListData.OpaqueAlphaTestGBuffer.ListParams, passData.RendererLists);
			CreateOverdrawRendererLists(waaaghRenderingData.RenderGraph, waaaghRendererListData.OpaqueDistortionGBuffer.ListParams, passData.RendererLists);
		}
		if (flag)
		{
			CreateOverdrawRendererLists(waaaghRenderingData.RenderGraph, waaaghRendererListData.Transparent.ListParams, passData.RendererLists);
		}
		TextureHandle input = waaaghResourceData.CameraDepthBuffer;
		builder.UseDepthBuffer(in input, DepthAccess.Read);
		passData.FullScreenDebugBuffer = builder.WriteBuffer(in m_Resources.FullScreenDebugBuffer);
		builder.AllowRendererListCulling(value: false);
		foreach (RendererListHandle rendererList in passData.RendererLists)
		{
			RendererListHandle input2 = rendererList;
			builder.UseRendererList(in input2);
		}
	}

	protected override void Render(PassData data, RenderGraphContext context)
	{
		context.cmd.SetRandomWriteTarget(1, data.FullScreenDebugBuffer);
		foreach (RendererListHandle rendererList in data.RendererLists)
		{
			context.cmd.DrawRendererList(rendererList);
		}
		context.renderContext.ExecuteCommandBuffer(context.cmd);
		context.cmd.Clear();
	}

	private void CreateOverdrawRendererLists(RenderGraph renderGraph, RendererListParams baseParams, List<RendererListHandle> results)
	{
		RendererListParams desc = baseParams;
		desc.drawSettings.perObjectData = PerObjectData.None;
		desc.filteringSettings.batchLayerMask = 128u;
		results.Add(renderGraph.CreateRendererList(in desc));
		desc.drawSettings.overrideShader = m_Shader;
		desc.filteringSettings.batchLayerMask = 4294967155u;
		results.Add(renderGraph.CreateRendererList(in desc));
	}
}
