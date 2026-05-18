using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Recorders.PostProcess;

public static class MotionBlurRecorder
{
	private class MotionBlurPassData
	{
		internal TextureHandle destinationTexture;

		internal TextureHandle sourceTexture;

		internal TextureHandle motionVectors;

		internal Material material;

		internal int passIndex;

		internal Camera camera;

		internal float intensity;

		internal float clamp;

		internal bool enableAlphaOutput;
	}

	internal static readonly int k_ShaderPropertyId_ViewProjM = Shader.PropertyToID("_ViewProjM");

	internal static readonly int k_ShaderPropertyId_PrevViewProjM = Shader.PropertyToID("_PrevViewProjM");

	internal static readonly int k_ShaderPropertyId_ViewProjMStereo = Shader.PropertyToID("_ViewProjMStereo");

	internal static readonly int k_ShaderPropertyId_PrevViewProjMStereo = Shader.PropertyToID("_PrevViewProjMStereo");

	public static void Render(PostProcessor processor, RenderGraph renderGraph, WaaaghCameraData cameraData, in TextureHandle cameraMotionVectors, in TextureHandle cameraDepth)
	{
		Material cameraMotionBlur = processor.MatLib.CameraMotionBlur;
		RenderTextureDescriptor descriptor = processor.FrameState.Descriptor;
		MotionBlur motionBlur = processor.Overrides.MotionBlur;
		RenderTextureDescriptor compatibleDescriptor = PostProcessor.GetCompatibleDescriptor(descriptor, descriptor.width, descriptor.height, descriptor.graphicsFormat);
		TextureHandle input = processor.CameraStackTargets.CurrentPostProcessSource;
		TextureHandle textureHandle = RenderGraphUtility.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "_MotionBlurTarget", clear: true, FilterMode.Bilinear);
		TextureHandle input2 = cameraMotionVectors;
		MotionBlurMode value = motionBlur.mode.value;
		int value2 = (int)motionBlur.quality.value;
		value2 += ((value == MotionBlurMode.CameraAndObjects) ? 3 : 0);
		MotionBlurPassData passData;
		using (IRasterRenderGraphBuilder rasterRenderGraphBuilder = renderGraph.AddRasterRenderPass<MotionBlurPassData>("Motion Blur", out passData, WaaaghProfileId.MotionBlur.Sampler(), ".\\Library\\PackageCache\\com.owlcat.visual@4f4b3d807b8a\\Runtime\\Waaagh\\Recorders\\PostProcess\\MotionBlurRecorder.cs", 53))
		{
			rasterRenderGraphBuilder.AllowGlobalStateModification(value: true);
			passData.destinationTexture = textureHandle;
			rasterRenderGraphBuilder.SetRenderAttachment(textureHandle, 0);
			passData.sourceTexture = input;
			rasterRenderGraphBuilder.UseTexture(in input);
			passData.motionVectors = input2;
			rasterRenderGraphBuilder.UseTexture(in input2);
			rasterRenderGraphBuilder.UseTexture(in cameraDepth);
			passData.material = cameraMotionBlur;
			passData.passIndex = value2;
			passData.camera = cameraData.camera;
			passData.enableAlphaOutput = cameraData.isAlphaOutputEnabled;
			passData.intensity = motionBlur.intensity.value;
			passData.clamp = motionBlur.clamp.value;
			rasterRenderGraphBuilder.SetRenderFunc(delegate(MotionBlurPassData data, RasterGraphContext context)
			{
				RasterCommandBuffer cmd = context.cmd;
				RTHandle rTHandle = data.sourceTexture;
				UpdateMotionBlurMatrices(ref data.material, data.camera);
				data.material.SetFloat("_Intensity", data.intensity);
				data.material.SetFloat("_Clamp", data.clamp);
				CoreUtils.SetKeyword(data.material, "_ENABLE_ALPHA_OUTPUT", data.enableAlphaOutput);
				PostProcessUtils.SetSourceSize(cmd, data.sourceTexture);
				cmd.SetGlobalTexture(PostProcessor.ShaderIDs._MotionVectorTexture, data.motionVectors);
				Vector2 vector = (rTHandle.useScaling ? new Vector2(rTHandle.rtHandleProperties.rtHandleScale.x, rTHandle.rtHandleProperties.rtHandleScale.y) : Vector2.one);
				Blitter.BlitTexture(cmd, rTHandle, vector, data.material, data.passIndex);
			});
		}
		processor.CameraStackTargets.SetCurrentPostProcessSource(textureHandle);
	}

	internal static void UpdateMotionBlurMatrices(ref Material material, Camera camera)
	{
		MotionVectorsPersistentData motionVectorsPersistentData = null;
		if (camera.TryGetComponent<WaaaghAdditionalCameraData>(out var component))
		{
			motionVectorsPersistentData = component.MotionVectorsPersistentData;
		}
		if (motionVectorsPersistentData != null)
		{
			int num = 0;
			material.SetMatrix(k_ShaderPropertyId_PrevViewProjM, motionVectorsPersistentData.previousViewProjectionStereo[num]);
			material.SetMatrix(k_ShaderPropertyId_ViewProjM, motionVectorsPersistentData.viewProjectionStereo[num]);
		}
	}
}
