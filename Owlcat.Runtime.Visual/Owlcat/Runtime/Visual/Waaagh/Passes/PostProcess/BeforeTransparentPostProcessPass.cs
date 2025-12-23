using System;
using Owlcat.Runtime.Visual.Overrides;
using Owlcat.Runtime.Visual.Waaagh.FrameData;
using Owlcat.Runtime.Visual.Waaagh.PipelineResources;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class BeforeTransparentPostProcessPass : ScriptableRenderPass
{
	private class Materials : MaterialCollection<PostProcessRuntimeShaders>
	{
		public Material ScreenSpaceCloudShadows;

		public Material MaskedColorTransform;

		public override void Init(PostProcessRuntimeShaders resources)
		{
			ScreenSpaceCloudShadows = Load(resources.ScreenSpaceCloudShadowsShader);
			MaskedColorTransform = Load(resources.MaskedColorTransformPS);
		}
	}

	private static class ShaderConstants
	{
		public static readonly int _MaskedColorTransformParams = Shader.PropertyToID("_MaskedColorTransformParams");

		public static readonly int _Texture0 = Shader.PropertyToID("_Texture0");

		public static readonly int _Texture1 = Shader.PropertyToID("_Texture1");

		public static readonly int _Texture0ScaleBias = Shader.PropertyToID("_Texture0ScaleBias");

		public static readonly int _Texture1ScaleBias = Shader.PropertyToID("_Texture1ScaleBias");

		public static readonly int _Texture0Color = Shader.PropertyToID("_Texture0Color");

		public static readonly int _Texture1Color = Shader.PropertyToID("_Texture1Color");

		public static readonly int _Intensity = Shader.PropertyToID("_Intensity");
	}

	private class PostProcessPassDataBase : PassDataBase
	{
		public TextureHandle Source;

		public TextureHandle Destination;
	}

	private class SSCSPassData : PostProcessPassDataBase
	{
		public TextureHandle CameraDepthRT;

		public TextureHandle CameraBakedGIRT;

		public Texture CloudTexture0;

		public Texture CloudTexture1;

		public Vector4 Tex0ScaleBias;

		public Vector4 Tex1ScaleBias;

		public Vector4 Tex0Color;

		public Vector4 Tex1Color;

		public float Intensity;

		public Material Material;
	}

	private class MaskedColorTransformPassData : PostProcessPassDataBase
	{
		public TextureHandle CameraDepthRT;

		public Material BlitMaterial;

		public Material ColorTransformMaterial;

		public float StencilRef;

		public Vector4 MaskedColorTransformParams;
	}

	private MaterialLibrary<Materials, PostProcessRuntimeShaders> m_MaterialLibrary;

	private Materials m_Materials;

	private Material m_BlitMaterial;

	private ScreenSpaceCloudShadows m_Sscs;

	private MaskedColorTransform m_MaskedColorTransform;

	private Vector2 m_CloudScroll0;

	private Vector2 m_CloudScroll1;

	public override string Name => "BeforeTransparentPostProcessPass";

	public BeforeTransparentPostProcessPass(RenderPassEvent evt, PostProcessResources resources, Material blitMaterial)
		: base(evt)
	{
		m_MaterialLibrary = new MaterialLibrary<Materials, PostProcessRuntimeShaders>(resources.Shaders);
		m_BlitMaterial = blitMaterial;
	}

	public override void RecordRenderGraph(ContextContainer frameData)
	{
		WaaaghCameraData cameraData = frameData.Get<WaaaghCameraData>();
		WaaaghRenderingData waaaghRenderingData = frameData.Get<WaaaghRenderingData>();
		RenderGraph renderGraph = waaaghRenderingData.RenderGraph;
		WaaaghResourceData waaaghResourceData = frameData.Get<WaaaghResourceData>();
		m_Materials = m_MaterialLibrary.Get(cameraData.camera);
		VolumeStack stack = VolumeManager.instance.stack;
		m_Sscs = stack.GetComponent<ScreenSpaceCloudShadows>();
		m_MaskedColorTransform = stack.GetComponent<MaskedColorTransform>();
		bool num = m_Sscs.IsActive();
		bool flag = m_MaskedColorTransform.IsActive();
		ProfilingSampler sampler = ProfilingSampler.Get(WaaaghProfileId.RenderBeforeTransparentPostProcess);
		renderGraph.BeginProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\BeforeTransparentPostProcessPass.cs", 84);
		TextureHandle source = waaaghResourceData.CameraColorBuffer;
		TextureHandle destination = TextureHandle.nullHandle;
		if (num)
		{
			RenderSSCS(renderGraph, waaaghResourceData, GetSource(), GetDestination());
		}
		if (flag)
		{
			RenderMaskedColorTransform(renderGraph, waaaghResourceData, GetSource(), GetDestination());
			Swap();
		}
		PostProcessPassDataBase passData2;
		RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<PostProcessPassDataBase>("RenderBeforeTransparentPostProcess.FinalBlit", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\BeforeTransparentPostProcessPass.cs", 117);
		try
		{
			PostProcessPassDataBase postProcessPassDataBase = passData2;
			TextureHandle input = GetSource();
			postProcessPassDataBase.Source = renderGraphBuilder.ReadTexture(in input);
			PostProcessPassDataBase postProcessPassDataBase2 = passData2;
			input = waaaghResourceData.CameraColorBuffer;
			postProcessPassDataBase2.Destination = renderGraphBuilder.WriteTexture(in input);
			renderGraphBuilder.SetRenderFunc(delegate(PostProcessPassDataBase passData, RenderGraphContext context)
			{
				RenderTexture renderTexture = passData.Source;
				RenderTexture renderTexture2 = passData.Destination;
				if (renderTexture != renderTexture2)
				{
					Blitter.BlitCameraTexture(context.cmd, passData.Source, passData.Destination);
				}
			});
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
		renderGraph.EndProfilingSampler(sampler, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\BeforeTransparentPostProcessPass.cs", 134);
		TextureHandle GetDestination()
		{
			if (!destination.IsValid())
			{
				RenderTextureDescriptor compatibleDescriptor = PostProcessPass.GetCompatibleDescriptor(cameraData.cameraTargetDescriptor, cameraData.cameraTargetDescriptor.width, cameraData.cameraTargetDescriptor.height, cameraData.cameraTargetDescriptor.graphicsFormat);
				destination = WaaaghRenderer.CreateRenderGraphTexture(renderGraph, compatibleDescriptor, "BeforeTransparentPostProcessRT", clear: false, FilterMode.Bilinear);
			}
			return destination;
		}
		TextureHandle GetSource()
		{
			return source;
		}
		void Swap()
		{
			CoreUtils.Swap(ref source, ref destination);
		}
	}

	private void RenderSSCS(RenderGraph renderGraph, WaaaghResourceData resources, TextureHandle source, TextureHandle destination)
	{
		SSCSPassData passData2;
		using RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<SSCSPassData>("RenderBeforeTransparentPostProcess.SSCS", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\BeforeTransparentPostProcessPass.cs", 158);
		passData2.Source = renderGraphBuilder.ReadWriteTexture(in source);
		passData2.Destination = renderGraphBuilder.ReadWriteTexture(in destination);
		passData2.Material = m_Materials.ScreenSpaceCloudShadows;
		passData2.CameraDepthRT = renderGraphBuilder.ReadTexture(in resources.CameraDepthCopyRT);
		passData2.CameraBakedGIRT = renderGraphBuilder.ReadTexture(in resources.CameraBakedGIRT);
		passData2.CloudTexture0 = m_Sscs.Texture0.value;
		passData2.CloudTexture1 = m_Sscs.Texture1.value;
		Vector2 value = m_Sscs.Texture0Tiling.value;
		m_CloudScroll0 += m_Sscs.Texture0ScrollSpeed.value * Time.deltaTime;
		passData2.Tex0ScaleBias = new Vector4(value.x, value.y, m_CloudScroll0.x, m_CloudScroll0.y);
		passData2.Tex0Color = m_Sscs.Texture0Color.value;
		value = m_Sscs.Texture1Tiling.value;
		m_CloudScroll1 += m_Sscs.Texture1ScrollSpeed.value * Time.deltaTime;
		passData2.Tex1ScaleBias = new Vector4(value.x, value.y, m_CloudScroll1.x, m_CloudScroll1.y);
		passData2.Tex1Color = m_Sscs.Texture1Color.value;
		passData2.Intensity = m_Sscs.Intensity.value;
		renderGraphBuilder.SetRenderFunc(delegate(SSCSPassData passData, RenderGraphContext context)
		{
			context.cmd.SetGlobalTexture(ShaderConstants._Texture0, passData.CloudTexture0);
			context.cmd.SetGlobalTexture(ShaderConstants._Texture1, passData.CloudTexture1);
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraDepthRT, passData.CameraDepthRT);
			context.cmd.SetGlobalTexture(ShaderPropertyId._CameraBakedGIRT, passData.CameraBakedGIRT);
			context.cmd.SetGlobalVector(ShaderConstants._Texture0ScaleBias, passData.Tex0ScaleBias);
			context.cmd.SetGlobalVector(ShaderConstants._Texture0Color, passData.Tex0Color);
			context.cmd.SetGlobalVector(ShaderConstants._Texture1ScaleBias, passData.Tex1ScaleBias);
			context.cmd.SetGlobalVector(ShaderConstants._Texture1Color, passData.Tex1Color);
			context.cmd.SetGlobalFloat(ShaderConstants._Intensity, passData.Intensity);
			context.cmd.SetRenderTarget(passData.Source);
			context.cmd.DrawProcedural(Matrix4x4.identity, passData.Material, 0, MeshTopology.Triangles, 3);
		});
	}

	private void RenderMaskedColorTransform(RenderGraph renderGraph, WaaaghResourceData resources, TextureHandle source, TextureHandle destination)
	{
		MaskedColorTransformPassData passData2;
		RenderGraphBuilder renderGraphBuilder = renderGraph.AddRenderPass<MaskedColorTransformPassData>("RenderBeforeTransparentPostProcess.MaskedColorTrasformPass", out passData2, ".\\Library\\PackageCache\\com.owlcat.visual@141c9a01de77\\Runtime\\Waaagh\\Passes\\PostProcess\\BeforeTransparentPostProcessPass.cs", 213);
		try
		{
			MaskedColorTransformPassData maskedColorTransformPassData = passData2;
			TextureHandle input = resources.CameraDepthBuffer;
			maskedColorTransformPassData.CameraDepthRT = renderGraphBuilder.ReadTexture(in input);
			passData2.Source = renderGraphBuilder.ReadWriteTexture(in source);
			passData2.Destination = renderGraphBuilder.ReadWriteTexture(in destination);
			passData2.ColorTransformMaterial = m_Materials.MaskedColorTransform;
			passData2.BlitMaterial = m_BlitMaterial;
			passData2.StencilRef = (float)m_MaskedColorTransform.StencilRef.value;
			passData2.MaskedColorTransformParams = new Vector4(m_MaskedColorTransform.Brightness.value, m_MaskedColorTransform.Contrast.value + 1f, 0f, 0f);
			renderGraphBuilder.SetRenderFunc(delegate(MaskedColorTransformPassData passData, RenderGraphContext context)
			{
				Vector4 value = new Vector4(1f, 1f, 0f, 0f);
				context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, passData.Source);
				context.cmd.SetGlobalVector(ShaderPropertyId._BlitScaleBias, value);
				context.cmd.SetGlobalFloat(ShaderPropertyId._BlitMipLevel, 0f);
				context.cmd.SetRenderTarget(passData.Destination, passData.CameraDepthRT);
				context.cmd.DrawProcedural(Matrix4x4.identity, passData.BlitMaterial, 0, MeshTopology.Triangles, 3, 1);
				context.cmd.SetGlobalFloat(ShaderPropertyId._StencilRef, passData.StencilRef);
				context.cmd.SetGlobalVector(ShaderConstants._MaskedColorTransformParams, passData.MaskedColorTransformParams);
				context.cmd.SetGlobalTexture(ShaderPropertyId._BlitTexture, passData.Source);
				context.cmd.SetRenderTarget(passData.Destination, passData.CameraDepthRT);
				context.cmd.DrawProcedural(Matrix4x4.identity, passData.ColorTransformMaterial, 0, MeshTopology.Triangles, 3, 1);
			});
		}
		finally
		{
			((IDisposable)renderGraphBuilder).Dispose();
		}
	}

	internal void Cleanup()
	{
		m_MaterialLibrary.Cleanup();
	}
}
