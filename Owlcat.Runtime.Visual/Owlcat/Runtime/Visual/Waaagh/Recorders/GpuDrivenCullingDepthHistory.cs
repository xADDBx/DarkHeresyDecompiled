using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.History;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class GpuDrivenCullingDepthHistory
{
	private sealed class PassData
	{
		public bool Cull;

		public TextureHandle Destination;

		public BufferHandle GlobalAtomicCounterBuffer;

		public int HistoryMipLevel;

		public DepthPyramidGenerationUtils.PyramidParameters PyramidParameters;

		public TextureHandle PyramidUAV;

		public TextureHandle Source;
	}

	public static void Record(in RecordContext context, DepthPyramidGenerationUtils pyramidGenerationUtils)
	{
		PassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<PassData>("GpuDriven.DepthHistory", out passData, WaaaghProfileId.GpuDrivenDepthHistory.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@7d4d1c447cd1\\Runtime\\Waaagh\\Recorders\\GpuDriven\\GpuDrivenCullingDepthHistory.cs", 14);
		ref readonly WaaaghCameraData cameraData = ref context.CameraData;
		ref readonly WaaaghRenderingData renderingData = ref context.RenderingData;
		ref readonly FrameResources frameResources = ref context.FrameResources;
		CullingDepthHistory cullingDepthHistory = cameraData.CullingDepthHistory;
		if (cullingDepthHistory == null)
		{
			passData.Cull = true;
			return;
		}
		int2 viewportSize = new int2(cameraData.scaledWidth, cameraData.scaledHeight);
		int lodCount = cullingDepthHistory.MipLevel + 1;
		pyramidGenerationUtils.PopulateGenerationData(ref passData.PyramidParameters, viewportSize, useMax: true, lodCount);
		passData.Cull = false;
		passData.Source = frameResources.CameraStackTargets.Depth;
		passData.Destination = renderingData.RenderGraph.ImportBackbuffer(cullingDepthHistory.GetTexture());
		passData.PyramidUAV = unsafeRenderGraphBuilder.CreateTransientTexture(in passData.PyramidParameters.PyramidDesc);
		passData.HistoryMipLevel = cullingDepthHistory.MipLevel;
		unsafeRenderGraphBuilder.UseTexture(in passData.Source);
		if (passData.PyramidParameters.GlobalAtomicCounterDesc.count > 0)
		{
			passData.GlobalAtomicCounterBuffer = unsafeRenderGraphBuilder.CreateTransientBuffer(in passData.PyramidParameters.GlobalAtomicCounterDesc);
		}
		else
		{
			passData.GlobalAtomicCounterBuffer = BufferHandle.nullHandle;
		}
		cullingDepthHistory.UpdateVersion();
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(PassData data, UnsafeGraphContext context)
		{
			Render(data, context);
		});
	}

	private static void Render(PassData data, UnsafeGraphContext context)
	{
		if (!data.Cull)
		{
			DepthPyramidGenerationUtils.Render(context, in data.PyramidParameters, data.Source, data.PyramidUAV, data.GlobalAtomicCounterBuffer);
			Vector4 vector = data.PyramidParameters.PyramidMipRects[data.HistoryMipLevel];
			int srcX = (int)vector.x;
			int srcY = (int)vector.y;
			int srcWidth = (int)vector.z;
			int srcHeight = (int)vector.w;
			context.cmd.CopyTexture(data.PyramidUAV, 0, 0, srcX, srcY, srcWidth, srcHeight, data.Destination, 0, 0, 0, 0);
		}
	}
}
