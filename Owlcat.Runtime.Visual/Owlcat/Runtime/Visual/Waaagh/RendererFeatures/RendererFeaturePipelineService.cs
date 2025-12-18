using System;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures;

internal class RendererFeaturePipelineService
{
	private readonly Func<bool> m_IsInitialized;

	private readonly Action m_OnCleanup;

	private readonly Action<WaaaghPipeline> m_OnInit;

	private bool m_IsOwner;

	public RendererFeaturePipelineService(Action<WaaaghPipeline> onInit, Action onCleanup, Func<bool> isInitialized)
	{
		m_OnInit = onInit;
		m_OnCleanup = onCleanup;
		m_IsInitialized = isInitialized;
	}

	public void OnCreate()
	{
		WaaaghPipeline.Created -= OnPipelineCreated;
		WaaaghPipeline.Created += OnPipelineCreated;
	}

	private void OnPipelineDestroying(WaaaghPipeline pipeline)
	{
		if (m_IsOwner)
		{
			m_IsOwner = false;
			m_OnCleanup();
		}
		WaaaghPipeline.Created -= OnPipelineCreated;
		WaaaghPipeline.Created += OnPipelineCreated;
		WaaaghPipeline.Destroying -= OnPipelineDestroying;
	}

	private void OnPipelineCreated(WaaaghPipeline pipeline)
	{
		WaaaghPipeline.Created -= OnPipelineCreated;
		if (!m_IsInitialized())
		{
			m_IsOwner = true;
			m_OnInit(pipeline);
			WaaaghPipeline.Destroying += OnPipelineDestroying;
		}
	}

	public void OnDispose()
	{
		WaaaghPipeline.Created -= OnPipelineCreated;
		WaaaghPipeline.Destroying -= OnPipelineDestroying;
		if (m_IsOwner)
		{
			m_OnCleanup();
			m_IsOwner = false;
		}
	}
}
