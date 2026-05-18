using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public sealed class MaterialLibrary : IDisposable
{
	public class SsrCompute
	{
		public ComputeShader CS { get; }

		public LocalKeyword SsrApproxKeyword { get; }

		public ComputeShaderKernelDescriptor RaytraceKernel { get; }

		public ComputeShaderKernelDescriptor ReprojectionKernel { get; }

		public ComputeShaderKernelDescriptor AccumulateKernel { get; }

		public ComputeShaderKernelDescriptor BlurKernel { get; }

		public SsrCompute(ComputeShader cs)
		{
			CS = cs;
			RaytraceKernel = cs.GetKernelDescriptor("ScreenSpaceReflectionsTracing");
			ReprojectionKernel = cs.GetKernelDescriptor("ScreenSpaceReflectionsReprojection");
			AccumulateKernel = cs.GetKernelDescriptor("ScreenSpaceReflectionsAccumulate");
			BlurKernel = cs.GetKernelDescriptor("ScreenSpaceBilateralBlur");
			SsrApproxKeyword = new LocalKeyword(cs, "SSR_APPROX");
		}
	}

	public class BilateralUpsampleCompute
	{
		public ComputeShader CS { get; }

		public ComputeShaderKernelDescriptor UpSampleColorKernel { get; }

		public BilateralUpsampleCompute(ComputeShader cs)
		{
			CS = cs;
			UpSampleColorKernel = cs.GetKernelDescriptor("BilateralUpSampleColor4");
		}
	}

	private List<Material> m_Materials = new List<Material>();

	public Material CopyDepth;

	public int CopyDepthToColorPass;

	public int CopyDepthToDepthPass;

	public int CopyDepthScaleOffsetPass;

	public Material FinalBlitMaterial;

	public int FinalBlitMaterialNearestPass;

	public int FinalBlitMaterialLinearPass;

	public Material FinalBlitHdrMaterial;

	public int FinalBlitHdrMaterialNearestPass;

	public int FinalBlitHdrMaterialLinearPass;

	public Material ErrorMaterial;

	public Material CopyCachedShadowsMaterial;

	public Material ColorPyramidMaterial;

	public Material ColorPyramidBlitMaterial;

	public Material ApplyDistortionMaterial;

	public Material DeferredReflectionsMaterial;

	public Material DecalBufferBlitMaterial;

	public int DecalBufferBlitMaterialUnpackPass;

	public int DecalBufferBlitMaterialPackPass;

	public Material BlitMaterial;

	public Material CameraMotionVectorsMaterial;

	public Material ObjectsMotionVectorsMaterial;

	public Material SsrMaterial;

	public int SsrBlurPass;

	public int SsrCompositeSsrPass;

	public Material DeferredFogMaterial;

	public Material DeferredLightingMaterial { get; }

	public SsrCompute SsrCS { get; }

	public BilateralUpsampleCompute BilateralUpsampleCS { get; }

	public MaterialLibrary(RenderRuntimeShaders shaders)
	{
		CopyDepth = CreateMat(shaders.CopyDepthSimplePS);
		CopyDepth.SetFloat("_DepthBlendOp", SystemInfo.usesReversedZBuffer ? 4 : 3);
		CopyDepthToColorPass = CopyDepth.FindPass("CopyDepthAsColor");
		CopyDepthToDepthPass = CopyDepth.FindPass("CopyDepth");
		CopyDepthScaleOffsetPass = CopyDepth.FindPass("CopyDepthScaleOffset");
		DeferredLightingMaterial = CreateMat(shaders.DeferredLightingShader);
		FinalBlitMaterial = CreateMat(shaders.CoreBlitPS);
		FinalBlitMaterialNearestPass = FinalBlitMaterial.FindPass("NearestDebugDraw");
		FinalBlitMaterialLinearPass = FinalBlitMaterial.FindPass("BilinearDebugDraw");
		FinalBlitHdrMaterial = CreateMat(shaders.BlitHDROverlayPS);
		FinalBlitHdrMaterialNearestPass = FinalBlitHdrMaterial.FindPass("NearestDebugDraw");
		FinalBlitHdrMaterialLinearPass = FinalBlitHdrMaterial.FindPass("BilinearDebugDraw");
		ErrorMaterial = CreateMat(Shader.Find("Hidden/InternalErrorShader"));
		CopyCachedShadowsMaterial = CreateMat(shaders.CopyCachedShadowsPS);
		ColorPyramidMaterial = CreateMat(shaders.ColorPyramidShader);
		ColorPyramidBlitMaterial = CreateMat(shaders.BlitShader);
		ApplyDistortionMaterial = CreateMat(shaders.ApplyDistortionShader);
		DeferredReflectionsMaterial = shaders.DeferredReflectionsMaterial;
		DecalBufferBlitMaterial = CreateMat(shaders.DBufferBlitShader);
		DecalBufferBlitMaterialUnpackPass = DecalBufferBlitMaterial.FindPass("UnpackGBuffer");
		DecalBufferBlitMaterialPackPass = DecalBufferBlitMaterial.FindPass("PackGBuffer");
		BlitMaterial = CreateMat(shaders.BlitShader);
		CameraMotionVectorsMaterial = CreateMat(shaders.CameraMotionVectorsPS);
		ObjectsMotionVectorsMaterial = CreateMat(shaders.ObjectMotionVectorsPS);
		SsrMaterial = CreateMat(shaders.ScreenSpaceReflectionsShaderPS);
		SsrBlurPass = SsrMaterial.FindPass("BilateralBlur");
		SsrCompositeSsrPass = SsrMaterial.FindPass("CompositeSSR");
		DeferredFogMaterial = CreateMat(shaders.FogShader);
		SsrCS = new SsrCompute(shaders.StochasticScreenSpaceReflectionsCS);
		BilateralUpsampleCS = new BilateralUpsampleCompute(shaders.BilateralUpsampleCS);
	}

	private Material CreateMat(Shader shader)
	{
		Material material = CoreUtils.CreateEngineMaterial(shader);
		m_Materials.Add(material);
		return material;
	}

	public void Dispose()
	{
		foreach (Material material in m_Materials)
		{
			CoreUtils.Destroy(material);
		}
		m_Materials.Clear();
	}
}
