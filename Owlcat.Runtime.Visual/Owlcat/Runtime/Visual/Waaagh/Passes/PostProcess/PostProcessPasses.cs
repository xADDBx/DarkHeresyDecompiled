using System;
using Owlcat.Runtime.Visual.Waaagh.PostProcess;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.Passes.PostProcess;

public class PostProcessPasses : IDisposable
{
	private ColorGradingLutPass m_ColorGradingLutPass;

	private BeforeTransparentPostProcessPass m_BeforeTransparentPostProcessPass;

	private PostProcessPass m_PostProcessPass;

	private FinalPostProcessPass m_FinalPostProcessPass;

	private PostProcessResources m_RendererPostProcessData;

	private PostProcessResources m_CurrentPostProcessData;

	private Material m_BlitMaterial;

	public bool IsCreated => m_CurrentPostProcessData != null;

	public ColorGradingLutPass ColorGradingLutPass => m_ColorGradingLutPass;

	public BeforeTransparentPostProcessPass BeforeTransparentPostProcessPass => m_BeforeTransparentPostProcessPass;

	public PostProcessPass PostProcessPass => m_PostProcessPass;

	public FinalPostProcessPass FinalPostProcessPass => m_FinalPostProcessPass;

	public PostProcessPasses(PostProcessResources resources, ref PostProcessParams parameters)
	{
		m_RendererPostProcessData = resources;
		m_BlitMaterial = parameters.BlitMaterial;
		Recreate(resources, ref parameters);
	}

	public void Recreate(PostProcessResources resources, ref PostProcessParams parameters)
	{
		if (m_RendererPostProcessData != null)
		{
			resources = m_RendererPostProcessData;
		}
		if (resources != m_CurrentPostProcessData)
		{
			if (m_CurrentPostProcessData != null)
			{
				m_ColorGradingLutPass?.Cleanup();
				m_BeforeTransparentPostProcessPass?.Cleanup();
				m_PostProcessPass?.Cleanup();
				m_FinalPostProcessPass?.Cleanup();
				m_ColorGradingLutPass = null;
				m_BeforeTransparentPostProcessPass = null;
				m_PostProcessPass = null;
				m_FinalPostProcessPass = null;
				m_CurrentPostProcessData = null;
			}
			if (resources != null)
			{
				m_ColorGradingLutPass = new ColorGradingLutPass(RenderPassEvent.BeforeRenderingPrePasses, resources);
				m_BeforeTransparentPostProcessPass = new BeforeTransparentPostProcessPass(RenderPassEvent.BeforeRenderingOpaques, resources, m_BlitMaterial);
				m_PostProcessPass = new PostProcessPass(RenderPassEvent.BeforeRenderingPostProcessing, resources, ref parameters);
				m_FinalPostProcessPass = new FinalPostProcessPass(RenderPassEvent.AfterRenderingPostProcessing, resources, m_BlitMaterial);
				m_CurrentPostProcessData = resources;
			}
		}
	}

	public void Dispose()
	{
		m_ColorGradingLutPass?.Cleanup();
		m_BeforeTransparentPostProcessPass?.Cleanup();
		m_PostProcessPass?.Cleanup();
		m_FinalPostProcessPass?.Cleanup();
	}
}
