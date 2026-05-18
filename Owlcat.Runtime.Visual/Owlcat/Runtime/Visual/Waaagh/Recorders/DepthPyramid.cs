using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class DepthPyramid
{
	private static class ShaderConstantsId
	{
		public static readonly int _DepthPyramidSamplingRatio = Shader.PropertyToID("_DepthPyramidSamplingRatio");

		public static readonly int _DepthPyramidMipRects = Shader.PropertyToID("_DepthPyramidMipRects");

		public static readonly int _DepthPyramidLodCount = Shader.PropertyToID("_DepthPyramidLodCount");
	}

	private class DepthPyramidPassData
	{
		public TextureHandle CameraDepthBuffer;

		public DepthPyramidGenerationUtils.PyramidParameters PyramidParameters;

		public TextureHandle DepthPyramidUAV;

		public GPUDrivenDepthReprojectionUtils.ReprojectionParameters DepthReprojectionParameters;

		public BufferHandle GlobalAtomicCounterBuffer;

		public TextureHandle PackedReprojectedDepth;
	}

	public static void BuildDepthPyramid(in RecordContext context, DepthPyramidGenerationUtils depthPyramidGenerationUtils)
	{
		BuildDepthPyramid(in context, depthPyramidGenerationUtils, useMax: false, TextureHandle.nullHandle, default(GPUDrivenDepthReprojectionUtils.ReprojectionParameters));
	}

	public static void BuildDepthPyramid(in RecordContext context, DepthPyramidGenerationUtils depthPyramidGenerationUtils, bool useMax, TextureHandle packedReprojectedDepth, GPUDrivenDepthReprojectionUtils.ReprojectionParameters depthReprojectionParameters)
	{
		bool flag = packedReprojectedDepth.IsValid();
		string passName = (flag ? "DepthPyramid.Build" : "DepthPyramid.BuildReproject");
		DepthPyramidPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<DepthPyramidPassData>(passName, out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\DepthPyramid.cs", 52);
		depthPyramidGenerationUtils.PopulateGenerationData(ref passData.PyramidParameters, new int2(context.CameraData.cameraTargetDescriptor.width, context.CameraData.cameraTargetDescriptor.height), useMax);
		if (passData.PyramidParameters.GlobalAtomicCounterDesc.count > 0)
		{
			passData.GlobalAtomicCounterBuffer = unsafeRenderGraphBuilder.CreateTransientBuffer(in passData.PyramidParameters.GlobalAtomicCounterDesc);
		}
		TextureDesc desc = passData.PyramidParameters.PyramidDesc;
		desc.name = "CameraDepthPyramid";
		TextureHandle input = (passData.DepthPyramidUAV = context.RenderGraph.CreateTexture(in desc));
		unsafeRenderGraphBuilder.UseTexture(in passData.DepthPyramidUAV, AccessFlags.ReadWrite);
		passData.CameraDepthBuffer = context.FrameResources.CameraStackTargets.Depth;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraDepthBuffer);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in input, GlobalTextureShaderPropertyId._CameraDepthPyramidRT);
		if (flag)
		{
			passData.PackedReprojectedDepth = packedReprojectedDepth;
			unsafeRenderGraphBuilder.UseTexture(in packedReprojectedDepth);
			passData.DepthReprojectionParameters = depthReprojectionParameters;
		}
		else
		{
			passData.PackedReprojectedDepth = TextureHandle.nullHandle;
			passData.DepthReprojectionParameters = default(GPUDrivenDepthReprojectionUtils.ReprojectionParameters);
		}
		unsafeRenderGraphBuilder.SetRenderFunc<DepthPyramidPassData>(ExecutePass);
	}

	private static void ExecutePass(DepthPyramidPassData data, UnsafeGraphContext context)
	{
		if (!data.PackedReprojectedDepth.IsValid())
		{
			DepthPyramidGenerationUtils.Render(context, in data.PyramidParameters, data.CameraDepthBuffer, data.DepthPyramidUAV, data.GlobalAtomicCounterBuffer);
		}
		else
		{
			int historyMipLevel = data.DepthReprojectionParameters.HistoryMipLevel;
			int toMipExclusive = historyMipLevel + 1;
			DepthPyramidGenerationUtils.Render(context, in data.PyramidParameters, data.CameraDepthBuffer, data.DepthPyramidUAV, data.GlobalAtomicCounterBuffer, 1, toMipExclusive);
			Vector4 vector = data.PyramidParameters.PyramidMipRects[historyMipLevel];
			GPUDrivenDepthReprojectionUtils.UnpackReprojectedDepth(destinationViewport: new Rect(vector.x, vector.y, vector.z, vector.w), reprojectionParameters: in data.DepthReprojectionParameters, cmd: context.cmd, packedReprojectedDepth: data.PackedReprojectedDepth, destination: data.DepthPyramidUAV);
			int fromMip = historyMipLevel + 1;
			DepthPyramidGenerationUtils.Render(context, in data.PyramidParameters, data.CameraDepthBuffer, data.DepthPyramidUAV, data.GlobalAtomicCounterBuffer, fromMip);
		}
		context.cmd.SetGlobalVector(ShaderConstantsId._DepthPyramidSamplingRatio, data.PyramidParameters.PyramidSamplingRatio);
		context.cmd.SetGlobalVectorArray(ShaderConstantsId._DepthPyramidMipRects, data.PyramidParameters.PyramidMipRects);
		context.cmd.SetGlobalInt(ShaderConstantsId._DepthPyramidLodCount, data.PyramidParameters.PyramidLodCount);
	}
}
