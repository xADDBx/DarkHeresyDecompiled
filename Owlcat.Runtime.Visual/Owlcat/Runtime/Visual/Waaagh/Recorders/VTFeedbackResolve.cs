using Owlcat.Runtime.Visual.VirtualTexture;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

internal static class VTFeedbackResolve
{
	private sealed class ResolvePassData
	{
		public ComputeShader Shader;

		public TextureHandle VTFeedbackMRT;

		public BufferHandle PackedBuffer;

		public int2 TileAtlasSize;

		public int ScreenWidth;

		public int ScreenHeight;

		public int DispatchX;

		public int DispatchY;

		public bool ShouldDispatch;
	}

	private const int kResolveKernel = 1;

	private static readonly int s_VTFeedbackBuffer = Shader.PropertyToID("_VTFeedbackBuffer");

	private static readonly int s_VTFeedbackMRT = Shader.PropertyToID("_VTFeedbackMRT");

	private static readonly int s_VirtualAtlasWidthInTiles = Shader.PropertyToID("_VirtualAtlasWidthInTiles");

	private static readonly int s_VirtualAtlasHeightInTiles = Shader.PropertyToID("_VirtualAtlasHeightInTiles");

	private static readonly int s_ScreenWidth = Shader.PropertyToID("_ScreenWidth");

	private static readonly int s_ScreenHeight = Shader.PropertyToID("_ScreenHeight");

	public static void Record(in RecordContext context, bool shouldDispatch)
	{
		VirtualTextureManager virtualTextureManager = context.VirtualTextureManager;
		if (virtualTextureManager == null || virtualTextureManager.IsVirtualAtlasEmpty)
		{
			return;
		}
		ComputeShader computeShader = virtualTextureManager.PipelineResources?.VTFeedbackCS;
		if (computeShader == null)
		{
			return;
		}
		VTFeedbackData vTFeedbackData = context.FrameResources.VTFeedbackData;
		if (!vTFeedbackData.VTFeedbackMRT.IsValid() || !vTFeedbackData.VTPackedFeedbackBuffer.IsValid())
		{
			return;
		}
		int2 virtualAtlasResolutionInTiles = virtualTextureManager.VirtualAtlasResolutionInTiles;
		if (virtualAtlasResolutionInTiles.x <= 0 || virtualAtlasResolutionInTiles.y <= 0)
		{
			return;
		}
		RenderTextureDescriptor cameraTargetDescriptor = context.CameraData.cameraTargetDescriptor;
		ResolvePassData passData;
		using IComputeRenderGraphBuilder computeRenderGraphBuilder = context.RenderGraph.AddComputePass<ResolvePassData>("VT Feedback Resolve", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\VTFeedbackResolve.cs", 63);
		passData.Shader = computeShader;
		passData.VTFeedbackMRT = vTFeedbackData.VTFeedbackMRT;
		passData.PackedBuffer = vTFeedbackData.VTPackedFeedbackBuffer;
		passData.TileAtlasSize = virtualAtlasResolutionInTiles;
		passData.ScreenWidth = cameraTargetDescriptor.width;
		passData.ScreenHeight = cameraTargetDescriptor.height;
		passData.DispatchX = Mathf.CeilToInt((float)cameraTargetDescriptor.width / 8f);
		passData.DispatchY = Mathf.CeilToInt((float)cameraTargetDescriptor.height / 8f);
		passData.ShouldDispatch = shouldDispatch;
		computeRenderGraphBuilder.UseTexture(in passData.VTFeedbackMRT);
		computeRenderGraphBuilder.UseBuffer(in passData.PackedBuffer, AccessFlags.ReadWrite);
		computeRenderGraphBuilder.SetRenderFunc(delegate(ResolvePassData data, ComputeGraphContext context)
		{
			if (data.ShouldDispatch)
			{
				context.cmd.SetComputeBufferParam(data.Shader, 1, s_VTFeedbackBuffer, data.PackedBuffer);
				context.cmd.SetComputeTextureParam(data.Shader, 1, s_VTFeedbackMRT, data.VTFeedbackMRT);
				context.cmd.SetComputeIntParam(data.Shader, s_VirtualAtlasWidthInTiles, data.TileAtlasSize.x);
				context.cmd.SetComputeIntParam(data.Shader, s_VirtualAtlasHeightInTiles, data.TileAtlasSize.y);
				context.cmd.SetComputeIntParam(data.Shader, s_ScreenWidth, data.ScreenWidth);
				context.cmd.SetComputeIntParam(data.Shader, s_ScreenHeight, data.ScreenHeight);
				context.cmd.DispatchCompute(data.Shader, 1, data.DispatchX, data.DispatchY, 1);
			}
		});
	}
}
