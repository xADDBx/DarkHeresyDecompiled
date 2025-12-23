using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class RawHistoryPass : ScriptableRenderPass
{
	private class PassData
	{
		internal TextureHandle source;

		internal TextureHandle destination;

		internal bool useProceduralBlit;

		internal Material copyColorMaterial;

		internal int sampleOffsetShaderHandle;
	}

	private Material m_BlitMaterial;

	public override string Name => "RawHistoryPass";

	public RawHistoryPass(RenderPassEvent evt, Material blitMaterial)
		: base(evt)
	{
		m_BlitMaterial = blitMaterial;
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		if (waaaghCameraData.historyManager == null)
		{
			return;
		}
		WaaaghCameraHistory historyManager = waaaghCameraData.historyManager;
		if (historyManager.IsAccessRequested<RawColorHistory>())
		{
			RawColorHistory historyForWrite = historyManager.GetHistoryForWrite<RawColorHistory>();
			if (historyForWrite != null)
			{
				historyForWrite.Update(ref waaaghCameraData.cameraTargetDescriptor);
				if (historyForWrite.GetCurrentTexture() != null)
				{
					TextureHandle destination = waaaghRenderingData.RenderGraph.ImportTexture(historyForWrite.GetCurrentTexture());
					RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
					TextureHandle source = waaaghResourceData.CameraColorBuffer;
					RenderInternal(renderGraph, in destination, in source);
				}
			}
		}
		if (!historyManager.IsAccessRequested<RawDepthHistory>())
		{
			return;
		}
		RawDepthHistory historyForWrite2 = historyManager.GetHistoryForWrite<RawDepthHistory>();
		if (historyForWrite2 != null)
		{
			RenderTextureDescriptor cameraDesc = waaaghCameraData.cameraTargetDescriptor;
			cameraDesc.graphicsFormat = GraphicsFormat.R32_SFloat;
			cameraDesc.depthBufferBits = 0;
			historyForWrite2.Update(ref cameraDesc);
			if (historyForWrite2.GetCurrentTexture() != null)
			{
				TextureHandle destination2 = waaaghRenderingData.RenderGraph.ImportTexture(historyForWrite2.GetCurrentTexture());
				RenderGraph renderGraph2 = waaaghRenderingData.RenderGraph;
				TextureHandle source = waaaghResourceData.CameraDepthBuffer;
				RenderInternal(renderGraph2, in destination2, in source);
			}
		}
	}

	private void RenderInternal(RenderGraph renderGraph, in TextureHandle destination, in TextureHandle source)
	{
		PassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<PassData>(Name, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\RawHistoryPass.cs", 77);
		passData.destination = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.source = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.copyColorMaterial = m_BlitMaterial;
		if (destination.IsValid())
		{
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in destination, Shader.PropertyToID("_CameraOpaqueTexture"));
		}
		rasterRenderGraphBuilder.SetRenderFunc(delegate(PassData data, RasterGraphContext context)
		{
			ExecutePass(context.cmd, data);
		});
	}

	private static void ExecutePass(RasterCommandBuffer cmd, PassData data)
	{
		RTHandle rTHandle = data.source;
		Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		Blitter.BlitTexture(cmd, rTHandle, vector, data.copyColorMaterial, 0);
	}
}
