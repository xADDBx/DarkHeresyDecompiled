using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.GPUDrivenBRG;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.Waaagh;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Authoring;

internal class AuthoringPropertyBlock
{
	private readonly Renderer m_Renderer;

	[CanBeNull]
	private GPUDrivenRenderer m_GPUDrivenRenderer;

	[CanBeNull]
	private MaterialPropertyBlock m_MaterialPropertyBlock;

	[NotNull]
	private MaterialPropertyBlock MaterialPropertyBlock => m_MaterialPropertyBlock ?? (m_MaterialPropertyBlock = new MaterialPropertyBlock());

	private GPUDrivenRenderer GPUDrivenRenderer
	{
		get
		{
			if (m_GPUDrivenRenderer != null)
			{
				return m_GPUDrivenRenderer;
			}
			if (!m_Renderer.TryGetComponent<GPUDrivenRenderer>(out var component))
			{
				return m_Renderer.gameObject.AddComponent<GPUDrivenRenderer>();
			}
			return component;
		}
	}

	public AuthoringPropertyBlock(Renderer renderer)
	{
		m_Renderer = renderer;
	}

	public void SetFloat(int nameID, float value)
	{
		if (!(m_Renderer == null))
		{
			if (UseBRG())
			{
				GPUDrivenRenderer gPUDrivenRenderer = GPUDrivenRenderer;
				GPUDrivenRenderer.PropertyData data = GPUDrivenRenderer.PropertyData.Float(nameID, value);
				gPUDrivenRenderer.AddPropertyData(in data);
			}
			else
			{
				MaterialPropertyBlock.SetFloat(nameID, value);
			}
		}
	}

	public void SetMatrix(int nameID, Matrix4x4 matrix)
	{
		if (!(m_Renderer == null))
		{
			if (UseBRG())
			{
				GPUDrivenRenderer gPUDrivenRenderer = GPUDrivenRenderer;
				GPUDrivenRenderer.PropertyData data = GPUDrivenRenderer.PropertyData.Matrix(nameID, matrix);
				gPUDrivenRenderer.AddPropertyData(in data);
			}
			else
			{
				MaterialPropertyBlock.SetMatrix(nameID, matrix);
			}
		}
	}

	public void Apply()
	{
		if (!(m_Renderer == null))
		{
			if (UseBRG())
			{
				GPUDrivenRenderer.MarkDataDirty();
			}
			else
			{
				m_Renderer.SetPropertyBlock(MaterialPropertyBlock);
			}
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool UseBRG()
	{
		if (WaaaghPipeline.Asset.GPUDrivenBRGSettings.IsEnabledAndSupported && m_Renderer is MeshRenderer)
		{
			return RendererUtils.AllowGPUDrivenRendering(m_Renderer);
		}
		return false;
	}
}
