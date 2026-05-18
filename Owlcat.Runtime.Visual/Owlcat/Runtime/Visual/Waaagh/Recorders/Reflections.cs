using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.BilateralUpsample;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Lighting;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders;

public static class Reflections
{
	private class SetupReflectionProbesPassData
	{
		public WaaaghReflectionProbes ReflectionProbes;
	}

	private class DeferredReflectionsPassData
	{
		public Material Material;

		public TextureHandle CameraColorRT;

		public TextureHandle CameraDeferredReflectionsRT;

		public TextureHandle CameraDepthRT;

		public TextureHandle CameraDepthCopyRT;

		public TextureHandle SsrRT;

		public TextureHandle SsrUpsampledRT;

		public int2 UpsampledSize;

		public DeferredReflectionProbeBatcher ProbeBatcher;

		public bool SsrEnabled;

		public bool SsrNeedUpsamplePass;

		public ComputeShader BilateralUpsampleCS;

		public ComputeShaderKernelDescriptor BilateralUpSampleColorKernel;

		internal ShaderVariablesBilateralUpsample ShaderVariablesBilateralUpsample;

		public bool IsPreviewCamera;

		public NativeArray<VisibleReflectionProbe> VisibleReflectionProbes;
	}

	private static class Passes
	{
		public const int kDefaultCubemapPass = 0;

		public const int kReflectionProbePass = 1;

		public const int kSsrPass = 2;

		public const int kFinalBlendPass = 3;
	}

	public static void SetupReflectionProbesPass(in RecordContext context)
	{
		SetupReflectionProbesPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<SetupReflectionProbesPassData>("SetupReflectionProbesPass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Reflections.cs", 22);
		passData.ReflectionProbes = context.ReflectionProbes;
		TextureHandle input = context.RenderGraph.ImportTexture(context.ReflectionProbes.AtlasRTHandle);
		unsafeRenderGraphBuilder.UseTexture(in input, AccessFlags.Write);
		unsafeRenderGraphBuilder.SetGlobalTextureAfterPass(in input, GlobalTextureShaderPropertyId.waaagh_ReflProbes_Atlas);
		unsafeRenderGraphBuilder.AllowPassCulling(value: false);
		unsafeRenderGraphBuilder.SetRenderFunc(delegate(SetupReflectionProbesPassData data, UnsafeGraphContext context)
		{
			data.ReflectionProbes.BlitAndSetGlobals(CommandBufferHelpers.GetNativeCommandBuffer(context.cmd));
		});
	}

	public unsafe static void DeferredReflectionsPass(in RecordContext context)
	{
		WaaaghCameraData cameraData = context.CameraData;
		DeferredReflectionsPassData passData;
		using IUnsafeRenderGraphBuilder unsafeRenderGraphBuilder = context.RenderGraph.AddUnsafePass<DeferredReflectionsPassData>("DeferredReflectionsPass", out passData, ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\Reflections.cs", 74);
		passData.Material = context.MaterialLibrary.DeferredReflectionsMaterial;
		passData.CameraColorRT = context.FrameResources.CameraStackTargets.Color;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraColorRT, AccessFlags.Write);
		TextureDesc desc = RenderingUtils.CreateTextureDesc("CameraDeferredReflectionsRT", cameraData.cameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		passData.CameraDeferredReflectionsRT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc);
		passData.CameraDepthRT = context.FrameResources.CameraStackTargets.Depth;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraDepthRT);
		passData.CameraDepthCopyRT = context.FrameResources.CameraAdditionalTargets.DepthCopy;
		unsafeRenderGraphBuilder.UseTexture(in passData.CameraDepthCopyRT);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraDepthRT);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraSpecularRT);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraNormalsRT);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId._CameraTranslucencyRT);
		unsafeRenderGraphBuilder.UseGlobalTexture(GlobalTextureShaderPropertyId.waaagh_ReflProbes_Atlas);
		passData.VisibleReflectionProbes = context.RenderingData.CullResults.visibleReflectionProbes;
		DeferredReflectionProbeBatcher deferredReflectionProbeBatcher = context.DeferredReflectionProbeBatcher;
		deferredReflectionProbeBatcher.Clear();
		deferredReflectionProbeBatcher.Batch(passData.VisibleReflectionProbes);
		passData.ProbeBatcher = deferredReflectionProbeBatcher;
		passData.IsPreviewCamera = false;
		passData.SsrEnabled = cameraData.IsSSREnabled;
		if (passData.SsrEnabled)
		{
			passData.SsrRT = context.FrameResources.SsrTargets.SsrCurrent;
			unsafeRenderGraphBuilder.UseTexture(in passData.SsrRT);
			ScreenSpaceReflections component = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>();
			if (component == null)
			{
				passData.SsrNeedUpsamplePass = false;
			}
			else
			{
				passData.SsrNeedUpsamplePass = component.UseUpsamplePass.value && component.Quality.value == ScreenSpaceReflectionsQuality.Half;
			}
			if (passData.SsrNeedUpsamplePass)
			{
				passData.BilateralUpsampleCS = context.MaterialLibrary.BilateralUpsampleCS.CS;
				passData.BilateralUpSampleColorKernel = context.MaterialLibrary.BilateralUpsampleCS.UpSampleColorKernel;
				TextureDesc desc2 = RenderingUtils.CreateTextureDesc("SsrUpsampledRT", cameraData.cameraTargetDescriptor);
				desc2.filterMode = FilterMode.Bilinear;
				desc2.wrapMode = TextureWrapMode.Clamp;
				desc2.width = cameraData.cameraTargetDescriptor.width;
				desc2.height = cameraData.cameraTargetDescriptor.height;
				desc2.enableRandomWrite = true;
				if (component.ColorPrecision.value == ColorPrecision.Color32)
				{
					desc2.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
				}
				else
				{
					desc2.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
				}
				passData.SsrUpsampledRT = unsafeRenderGraphBuilder.CreateTransientTexture(in desc2);
				passData.ShaderVariablesBilateralUpsample._HalfScreenSize = new Vector4(desc2.width / 2, desc2.height / 2, 1f / ((float)desc2.width * 0.5f), 1f / ((float)desc2.height * 0.5f));
				for (int i = 0; i < 16; i++)
				{
					passData.ShaderVariablesBilateralUpsample._DistanceBasedWeights[i] = Owlcat.Runtime.Visual.Waaagh.BilateralUpsample.BilateralUpsample.DistanceBasedWeights_2x2[i];
				}
				for (int j = 0; j < 32; j++)
				{
					passData.ShaderVariablesBilateralUpsample._TapOffsets[j] = Owlcat.Runtime.Visual.Waaagh.BilateralUpsample.BilateralUpsample.TapOffsets_2x2[j];
				}
				passData.UpsampledSize = new int2(desc2.width, desc2.height);
			}
		}
		unsafeRenderGraphBuilder.SetRenderFunc<DeferredReflectionsPassData>(ExecuteDeferredReflectionsPass);
	}

	private static void ExecuteDeferredReflectionsPass(DeferredReflectionsPassData data, UnsafeGraphContext context)
	{
		context.cmd.SetRenderTarget(data.CameraDeferredReflectionsRT, data.CameraDepthRT);
		RenderDefaultReflections(data, context);
		RenderVisibleReflectionProbes(data, context);
		if (data.SsrEnabled)
		{
			RenderSSR(data, context);
		}
		FinalBlendToCameraColor(data, context);
	}

	private static void RenderDefaultReflections(DeferredReflectionsPassData data, UnsafeGraphContext context)
	{
		GetDefaultReflectionProbe(data, out var cubemap, out var hdrDecodeValues);
		context.cmd.SetGlobalTexture(ShaderPropertyId.custom_SpecCube0, cubemap);
		context.cmd.SetGlobalVector(ShaderPropertyId.custom_SpecCube0_HDR, hdrDecodeValues);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}

	private static void RenderVisibleReflectionProbes(DeferredReflectionsPassData data, UnsafeGraphContext context)
	{
		CommandBuffer nativeCommandBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);
		data.ProbeBatcher.Render(data.VisibleReflectionProbes, nativeCommandBuffer, data.Material, 1);
	}

	private static void GetDefaultReflectionProbe(DeferredReflectionsPassData data, out Texture cubemap, out Vector4 hdrDecodeValues)
	{
		cubemap = ReflectionProbe.defaultTexture;
		hdrDecodeValues = ReflectionProbe.defaultTextureHDRDecodeValues;
	}

	private static void RenderSSR(DeferredReflectionsPassData data, UnsafeGraphContext context)
	{
		if (data.SsrNeedUpsamplePass)
		{
			if (FrameDebugger.enabled)
			{
				context.cmd.SetRenderTarget(data.SsrUpsampledRT);
			}
			int3 dispatchSize = data.BilateralUpSampleColorKernel.GetDispatchSize(data.UpsampledSize.x, data.UpsampledSize.y, 1);
			ConstantBuffer.PushGlobal(context.cmd, in data.ShaderVariablesBilateralUpsample, ShaderPropertyId.ShaderVariablesBilateralUpsample);
			context.cmd.SetComputeTextureParam(data.BilateralUpsampleCS, data.BilateralUpSampleColorKernel.Index, ShaderPropertyId._DepthTexture, data.CameraDepthCopyRT);
			context.cmd.SetComputeTextureParam(data.BilateralUpsampleCS, data.BilateralUpSampleColorKernel.Index, ShaderPropertyId._LowResolutionTexture, data.SsrRT);
			context.cmd.SetComputeTextureParam(data.BilateralUpsampleCS, data.BilateralUpSampleColorKernel.Index, ShaderPropertyId._OutputUpscaledTexture, data.SsrUpsampledRT);
			context.cmd.DispatchCompute(data.BilateralUpsampleCS, data.BilateralUpSampleColorKernel.Index, dispatchSize.x, dispatchSize.y, dispatchSize.z);
			if (FrameDebugger.enabled)
			{
				context.cmd.SetRenderTarget(data.CameraDeferredReflectionsRT, data.CameraDepthRT);
			}
			context.cmd.SetGlobalTexture(ShaderPropertyId._SsrRT, data.SsrUpsampledRT);
		}
		else
		{
			context.cmd.SetGlobalTexture(ShaderPropertyId._SsrRT, data.SsrRT);
		}
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 2, MeshTopology.Triangles, 3);
	}

	private static void FinalBlendToCameraColor(DeferredReflectionsPassData data, UnsafeGraphContext context)
	{
		context.cmd.SetRenderTarget(data.CameraColorRT, data.CameraDepthRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDeferredReflectionsRT, data.CameraDeferredReflectionsRT);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 3, MeshTopology.Triangles, 3);
	}
}
