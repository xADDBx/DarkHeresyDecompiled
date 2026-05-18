using System;
using Owlcat.Runtime.Visual.CustomPostProcess;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.CustomPostProcess;

internal sealed class PassDescriptor : IDisposable
{
	private CustomPostProcessEffectPass m_Pass;

	public ProfilingSampler ProfilingSampler;

	public Material Material;

	public Shader Shader;

	public string Name => m_Pass.Name;

	public PassDescriptor(CustomPostProcessEffectPass pass)
	{
		m_Pass = pass;
		ProfilingSampler = new ProfilingSampler(pass.Name);
		Material = CoreUtils.CreateEngineMaterial(pass.Shader);
		Shader = pass.Shader;
	}

	public void Dispose()
	{
		if ((bool)Material)
		{
			CoreUtils.Destroy(Material);
		}
	}

	internal bool Validate()
	{
		if (m_Pass == null)
		{
			return false;
		}
		if (Material == null)
		{
			return false;
		}
		if (m_Pass.Shader != Material.shader)
		{
			return false;
		}
		return true;
	}

	public static implicit operator CustomPostProcessEffectPass(PassDescriptor descriptor)
	{
		return descriptor.m_Pass;
	}
}
