using System;
using System.Collections.Generic;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.FrameData;

public class PostProcessMaterialLibrary : IDisposable
{
	private List<Material> m_Materials = new List<Material>();

	public Material[] BloomUpsample;

	public Material LutBuilderLdr { get; }

	public Material LutBuilderHdr { get; }

	public Material StopNaN { get; }

	public Material GaussianDepthOfField { get; }

	public Material GaussianDepthOfFieldCoC { get; }

	public Material BokehDepthOfField { get; }

	public Material BokehDepthOfFieldCoC { get; }

	public Material BloomEnhanced { get; }

	public Material Bloom { get; }

	public Material SMAA { get; }

	public Material RadialBlur { get; }

	public Material PaniniProjection { get; }

	public Material CameraMotionBlur { get; }

	public Material TemporalAntialiasing { get; }

	public Material FinalPass { get; }

	public Material ScalingSetup { get; }

	public Material Easu { get; }

	public Material Uber { get; }

	public Material BlitMaterial { get; }

	public PostProcessMaterialLibrary(PostProcessRuntimeShaders shaders, Material blitMaterial)
	{
		BlitMaterial = blitMaterial;
		LutBuilderLdr = CreateMat(shaders.LutBuilderLdrPS);
		LutBuilderHdr = CreateMat(shaders.LutBuilderHdrPS);
		StopNaN = CreateMat(shaders.StopNanPS);
		GaussianDepthOfField = CreateMat(shaders.GaussianDepthOfFieldPS);
		GaussianDepthOfFieldCoC = CreateMat(shaders.GaussianDepthOfFieldPS);
		BokehDepthOfField = CreateMat(shaders.BokehDepthOfFieldPS);
		BokehDepthOfFieldCoC = CreateMat(shaders.BokehDepthOfFieldPS);
		BloomEnhanced = CreateMat(shaders.BloomEnhancedPS);
		Bloom = CreateMat(shaders.BloomPS);
		SMAA = CreateMat(shaders.SubpixelMorphologicalAntialiasingPS);
		RadialBlur = CreateMat(shaders.RadialBlurPS);
		PaniniProjection = CreateMat(shaders.PaniniProjectionPS);
		CameraMotionBlur = CreateMat(shaders.CameraMotionBlurPS);
		TemporalAntialiasing = CreateMat(shaders.TemporalAntialiasingPS);
		FinalPass = CreateMat(shaders.FinalPostPassPS);
		ScalingSetup = CreateMat(shaders.ScalingSetupPS);
		Easu = CreateMat(shaders.EasuPS);
		Uber = CreateMat(shaders.UberPostPS);
		BloomUpsample = new Material[16];
		for (uint num = 0u; num < 16; num++)
		{
			BloomUpsample[num] = CreateMat(shaders.BloomPS);
		}
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
