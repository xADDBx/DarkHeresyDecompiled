using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures;

public abstract class ScriptableRendererFeature : ScriptableObject, IDisposable
{
	[SerializeField]
	[HideInInspector]
	private bool m_Active = true;

	public bool isActive => m_Active;

	public abstract void Create();

	public abstract void AddRenderPasses(ScriptableRenderer renderer, ContextContainer frameData);

	private void OnEnable()
	{
		Create();
	}

	private void OnValidate()
	{
		Create();
	}

	public void SetActive(bool active)
	{
		m_Active = active;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool disposing)
	{
	}

	internal virtual void StartSetupJobs(ContextContainer frameData)
	{
	}

	internal virtual void CompleteSetupJobs()
	{
	}
}
