using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class HistoryPasses
{
	private class RawHistoryPassData
	{
		internal TextureHandle source;

		internal TextureHandle destination;

		internal bool useProceduralBlit;

		internal Material copyColorMaterial;

		internal int sampleOffsetShaderHandle;
	}

	public static void RecordRawHistoryPasses(in RecordContext context)
	{
		WaaaghCameraData cameraData = context.CameraData;
		WaaaghCameraHistory historyManager = cameraData.historyManager;
		if (historyManager == null)
		{
			return;
		}
		if (historyManager.IsAccessRequested<RawColorHistory>())
		{
			RawColorHistory historyForWrite = historyManager.GetHistoryForWrite<RawColorHistory>();
			if (historyForWrite != null)
			{
				historyForWrite.Update(ref cameraData.cameraTargetDescriptor);
				if (historyForWrite.GetCurrentTexture() != null)
				{
					TextureHandle destination = context.FrameResources.CameraAdditionalTargets.RawColorHistory;
					if (!destination.IsValid())
					{
						destination = context.RenderGraph.ImportTexture(historyForWrite.GetCurrentTexture());
					}
					TextureHandle source = context.FrameResources.CameraStackTargets.Color;
					RawHistoryPass(in context, in destination, in source);
				}
			}
		}
		if (!historyManager.IsAccessRequested<RawDepthHistory>())
		{
			return;
		}
		RawDepthHistory historyForWrite2 = historyManager.GetHistoryForWrite<RawDepthHistory>();
		if (historyForWrite2 == null)
		{
			return;
		}
		RenderTextureDescriptor cameraDesc = cameraData.cameraTargetDescriptor;
		cameraDesc.graphicsFormat = GraphicsFormat.R32_SFloat;
		cameraDesc.depthBufferBits = 0;
		historyForWrite2.Update(ref cameraDesc);
		if (historyForWrite2.GetCurrentTexture() != null)
		{
			TextureHandle destination2 = context.FrameResources.CameraAdditionalTargets.RawDepthHistory;
			if (!destination2.IsValid())
			{
				destination2 = context.RenderGraph.ImportTexture(historyForWrite2.GetCurrentTexture());
			}
			TextureHandle source = context.FrameResources.CameraStackTargets.Depth;
			RawHistoryPass(in context, in destination2, in source);
		}
	}

	private static void RawHistoryPass(in RecordContext context, in TextureHandle destination, in TextureHandle source)
	{
		RawHistoryPassData passData;
		using IRasterRenderGraphBuilder rasterRenderGraphBuilder = context.RenderGraph.AddRasterRenderPass<RawHistoryPassData>("RawHistoryPass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\HistoryPasses.cs", 79);
		passData.destination = destination;
		rasterRenderGraphBuilder.SetRenderAttachment(destination, 0);
		passData.source = source;
		rasterRenderGraphBuilder.UseTexture(in source);
		passData.copyColorMaterial = context.MaterialLibrary.BlitMaterial;
		if (destination.IsValid())
		{
			rasterRenderGraphBuilder.SetGlobalTextureAfterPass(in destination, Shader.PropertyToID("_CameraOpaqueTexture"));
		}
		rasterRenderGraphBuilder.SetRenderFunc<RawHistoryPassData>(ExecutePass);
	}

	private static void ExecutePass(RawHistoryPassData data, RasterGraphContext context)
	{
		RTHandle rTHandle = data.source;
		Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
		Blitter.BlitTexture(context.cmd, rTHandle, vector, data.copyColorMaterial, 0);
	}
}
