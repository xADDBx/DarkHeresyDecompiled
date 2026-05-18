using System;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.CustomPostProcess;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess;

internal sealed class EffectDescriptor : IDisposable
{
	private CustomPostProcessEffect m_Effect;

	public ProfilingSampler ProfilingSampler;

	public CustomPostProcessRenderEvent Event;

	public List<PassDescriptor> Passes;

	public bool UseStencilMask => m_Effect.UseStencilMask;

	public EffectDescriptor(CustomPostProcessEffect effect)
	{
		m_Effect = effect;
		ProfilingSampler = new ProfilingSampler(effect.Name);
		Event = m_Effect.Event;
		Passes = effect.Passes.Select((CustomPostProcessEffectPass pass) => new PassDescriptor(pass)).ToList();
	}

	public void Dispose()
	{
		foreach (PassDescriptor pass in Passes)
		{
			pass.Dispose();
		}
	}

	internal bool Validate()
	{
		if (!m_Effect)
		{
			return false;
		}
		if (m_Effect.Event != Event)
		{
			return false;
		}
		foreach (PassDescriptor pass in Passes)
		{
			if (!pass.Validate())
			{
				return false;
			}
		}
		return true;
	}

	public static implicit operator CustomPostProcessEffect(EffectDescriptor descriptor)
	{
		return descriptor.m_Effect;
	}
}
