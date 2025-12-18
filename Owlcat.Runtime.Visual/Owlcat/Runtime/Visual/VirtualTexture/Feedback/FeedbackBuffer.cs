using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture.Feedback;

public class FeedbackBuffer : IDisposable
{
	public GraphicsBuffer PackedFeedbackBufferUAV { get; private set; }

	public RTHandle FeedbackRT { get; private set; }

	public FeedbackBuffer()
	{
		PackedFeedbackBufferUAV = new GraphicsBuffer(GraphicsBuffer.Target.Structured, 1, Marshal.SizeOf<uint>());
		PackedFeedbackBufferUAV.name = "VTPackedFeedbackBufferUAV_Dummy";
		FeedbackRT = AllocFeedbackRT(1, 1, "VTFeedbackRT_Dummy");
	}

	public void Dispose()
	{
		PackedFeedbackBufferUAV?.Dispose();
		if (FeedbackRT != null)
		{
			RTHandles.Release(FeedbackRT);
			FeedbackRT = null;
		}
	}

	private static RTHandle AllocFeedbackRT(int width, int height, string name)
	{
		return RTHandles.Alloc(width, height, 1, DepthBits.None, GraphicsFormat.R8_UInt, FilterMode.Point, TextureWrapMode.Repeat, TextureDimension.Tex2D, enableRandomWrite: true, useMipMap: false, autoGenerateMips: true, isShadowMap: false, 1, 0f, MSAASamples.None, bindTextureMS: false, useDynamicScale: false, useDynamicScaleExplicit: false, RenderTextureMemoryless.None, VRTextureUsage.None, name);
	}

	internal void Refresh(int2 virtualAtlasResolutionInTiles)
	{
		Dispose();
		int y = virtualAtlasResolutionInTiles.x * virtualAtlasResolutionInTiles.y;
		y = RenderingUtils.DivRoundUp(math.max(1, y), 32);
		PackedFeedbackBufferUAV = new GraphicsBuffer(GraphicsBuffer.Target.Structured, y, Marshal.SizeOf<uint>());
		PackedFeedbackBufferUAV.name = "VTPackedFeedbackBufferUAV";
		NativeArray<uint> data = new NativeArray<uint>(PackedFeedbackBufferUAV.count, Allocator.Temp);
		PackedFeedbackBufferUAV.SetData(data);
		data.Dispose();
		FeedbackRT = AllocFeedbackRT(virtualAtlasResolutionInTiles.x, virtualAtlasResolutionInTiles.y, "VTFeedbackRT");
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		commandBuffer.SetRenderTarget(FeedbackRT);
		commandBuffer.ClearRenderTarget(RTClearFlags.Color, Color.clear);
		Graphics.ExecuteCommandBuffer(commandBuffer);
		CommandBufferPool.Release(commandBuffer);
	}
}
