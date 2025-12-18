using JetBrains.Annotations;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.BilateralUpsample;
using Owlcat.Runtime.Visual.Waaagh.Data;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes;

public class DeferredReflectionsPass : ScriptableRenderPass<DeferredReflectionsPassData>
{
	private static class Passes
	{
		public const int kDefaultCubemapPass = 0;

		public const int kReflectionProbePass = 1;

		public const int kSsrPass = 2;

		public const int kFinalBlendPass = 3;
	}

	private readonly Material m_DeferredReflectionsMaterial;

	private readonly ComputeShader m_BilateralUpsampleCs;

	private readonly ComputeShaderKernelDescriptor m_BilateralUpSampleColorKernel;

	private readonly DeferredReflectionProbeBatcher m_ProbeBatcher;

	public override string Name => "DeferredReflectionsPass";

	private protected sealed override WaaaghProfileId? ProfileId => WaaaghProfileId.DeferredReflectionsPass;

	public DeferredReflectionsPass(RenderPassEvent evt, Material deferredReflectionsMaterial, DeferredReflectionProbeBatcher probeBatcher, ComputeShader bilateralUpsampleCs)
		: base(evt)
	{
		m_DeferredReflectionsMaterial = deferredReflectionsMaterial;
		m_ProbeBatcher = probeBatcher;
		m_BilateralUpsampleCs = bilateralUpsampleCs;
		m_BilateralUpSampleColorKernel = m_BilateralUpsampleCs.GetKernelDescriptor("BilateralUpSampleColor4");
	}

	public override void ConfigureRendererLists(ScriptableRenderContext context, ContextContainer frameData)
	{
		WaaaghRendererListData waaaghRendererListData = frameData.Get<WaaaghRendererListData>();
		DependsOn(in waaaghRendererListData.OpaqueGBuffer.List);
		DependsOn(in waaaghRendererListData.OpaqueAlphaTestGBuffer.List);
		DependsOn(in waaaghRendererListData.TerrainGBuffer.List);
	}

	protected unsafe override void Setup(RenderGraphBuilder builder, DeferredReflectionsPassData data, ContextContainer frameData)
	{
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		WaaaghCameraData waaaghCameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		TextureHandle input = waaaghResourceData.CameraColorBuffer;
		data.CameraColorRT = builder.WriteTexture(in input);
		TextureDesc desc = RenderingUtils.CreateTextureDesc("CameraDeferredReflectionsRT", waaaghCameraData.cameraTargetDescriptor);
		desc.colorFormat = GraphicsFormat.B10G11R11_UFloatPack32;
		data.CameraDeferredReflectionsRT = builder.CreateTransientTexture(in desc);
		input = waaaghResourceData.CameraDepthBuffer;
		data.CameraDepthRT = builder.UseDepthBuffer(in input, DepthAccess.Read);
		data.CameraDepthCopyRT = builder.ReadTexture(in waaaghResourceData.CameraDepthCopyRT);
		data.CameraNormalsRT = builder.ReadTexture(in waaaghResourceData.CameraNormalsRT);
		data.CameraTranslucencyRT = builder.ReadTexture(in waaaghResourceData.CameraTranslucencyRT);
		data.SsrEnabled = waaaghCameraData.IsSSREnabled;
		if (data.SsrEnabled)
		{
			data.SsrRT = builder.ReadTexture(in waaaghResourceData.SsrRT);
			ScreenSpaceReflections component = VolumeManager.instance.stack.GetComponent<ScreenSpaceReflections>();
			if (component == null)
			{
				data.SsrNeedUpsamplePass = false;
			}
			else
			{
				data.SsrNeedUpsamplePass = component.UseUpsamplePass.value && component.Quality.value == ScreenSpaceReflectionsQuality.Half;
			}
			if (data.SsrNeedUpsamplePass)
			{
				data.BilateralUpsampleCS = m_BilateralUpsampleCs;
				data.BilateralUpSampleColorKernel = m_BilateralUpSampleColorKernel;
				TextureDesc desc2 = RenderingUtils.CreateTextureDesc("SsrUpsampledRT", waaaghCameraData.cameraTargetDescriptor);
				desc2.filterMode = FilterMode.Bilinear;
				desc2.wrapMode = TextureWrapMode.Clamp;
				desc2.width = waaaghCameraData.cameraTargetDescriptor.width;
				desc2.height = waaaghCameraData.cameraTargetDescriptor.height;
				desc2.enableRandomWrite = true;
				if (component.ColorPrecision.value == ColorPrecision.Color32)
				{
					desc2.colorFormat = GraphicsFormat.R8G8B8A8_UNorm;
				}
				else
				{
					desc2.colorFormat = GraphicsFormat.R16G16B16A16_SFloat;
				}
				data.SsrUpsampledRT = builder.CreateTransientTexture(in desc2);
				data.ShaderVariablesBilateralUpsample._HalfScreenSize = new Vector4(desc2.width / 2, desc2.height / 2, 1f / ((float)desc2.width * 0.5f), 1f / ((float)desc2.height * 0.5f));
				for (int i = 0; i < 16; i++)
				{
					data.ShaderVariablesBilateralUpsample._DistanceBasedWeights[i] = Owlcat.Runtime.Visual.Waaagh.BilateralUpsample.BilateralUpsample.DistanceBasedWeights_2x2[i];
				}
				for (int j = 0; j < 32; j++)
				{
					data.ShaderVariablesBilateralUpsample._TapOffsets[j] = Owlcat.Runtime.Visual.Waaagh.BilateralUpsample.BilateralUpsample.TapOffsets_2x2[j];
				}
				data.UpsampledSize = new int2(desc2.width, desc2.height);
			}
		}
		data.VisibleReflectionProbes = waaaghRenderingData.CullResults.visibleReflectionProbes;
		data.IsPreviewCamera = false;
		m_ProbeBatcher.Clear();
		m_ProbeBatcher.Batch(data.VisibleReflectionProbes);
		data.ProbeBatcher = m_ProbeBatcher;
		data.Material = m_DeferredReflectionsMaterial;
		data.ActiveColorSpace = QualitySettings.activeColorSpace;
	}

	protected override void Render(DeferredReflectionsPassData data, RenderGraphContext context)
	{
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, data.CameraDepthCopyRT);
		context.cmd.SetRenderTarget(data.CameraDeferredReflectionsRT, data.CameraDepthRT);
		RenderDefaultReflections(data, in context);
		RenderVisibleReflectionProbes(data, in context);
		if (data.SsrEnabled)
		{
			RenderSSR(data, context);
		}
		FinalBlendToCameraColor(data, context);
	}

	private static void RenderDefaultReflections(DeferredReflectionsPassData data, in RenderGraphContext context)
	{
		GetDefaultReflectionProbe(data, out var cubemap, out var hdrDecodeValues);
		context.cmd.SetGlobalTexture(ShaderPropertyId.custom_SpecCube0, cubemap);
		context.cmd.SetGlobalVector(ShaderPropertyId.custom_SpecCube0_HDR, hdrDecodeValues);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 0, MeshTopology.Triangles, 3);
	}

	private static void GetDefaultReflectionProbe(DeferredReflectionsPassData data, [CanBeNull] out Texture cubemap, out Vector4 hdrDecodeValues)
	{
		if (data.IsPreviewCamera)
		{
			WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
			if (asset != null)
			{
				asset.GetDefaultPreviewReflectionProbe(out cubemap, out hdrDecodeValues);
				return;
			}
		}
		cubemap = ReflectionProbe.defaultTexture;
		hdrDecodeValues = ReflectionProbe.defaultTextureHDRDecodeValues;
	}

	private static void RenderVisibleReflectionProbes(DeferredReflectionsPassData data, in RenderGraphContext context)
	{
		data.ProbeBatcher.Render(data.VisibleReflectionProbes, context.cmd, data.Material, 1);
	}

	private static void RenderSSR(DeferredReflectionsPassData data, RenderGraphContext context)
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

	private static void FinalBlendToCameraColor(DeferredReflectionsPassData data, RenderGraphContext context)
	{
		context.cmd.SetRenderTarget(data.CameraColorRT, data.CameraDepthRT);
		context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDeferredReflectionsRT, data.CameraDeferredReflectionsRT);
		context.cmd.DrawProcedural(Matrix4x4.identity, data.Material, 3, MeshTopology.Triangles, 3);
	}
}
