using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations.GPUDriven;
using Owlcat.Runtime.Visual.Waaagh;
using Owlcat.Runtime.Visual.Waaagh.Data;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG;

public sealed class GPUDrivenMaterialPropertyBlock
{
	public enum Mode
	{
		Auto,
		BypassGPUDriven
	}

	[CanBeNull]
	private List<GPUDrivenRenderer.PropertyData> m_GPUDrivenPropertyData;

	[CanBeNull]
	private MaterialPropertyBlock m_NativePropertyBlock;

	private static bool UseGPUDriven([CanBeNull] Renderer renderer = null)
	{
		WaaaghPipelineAsset asset = WaaaghPipeline.Asset;
		if (asset != null && asset.GPUDrivenBRGSettings.IsEnabledAndSupported)
		{
			if (!(renderer == null))
			{
				if (renderer is MeshRenderer)
				{
					return RendererUtils.AllowGPUDrivenRendering(renderer);
				}
				return false;
			}
			return true;
		}
		return false;
	}

	public void Clear()
	{
		m_NativePropertyBlock?.Clear();
		m_GPUDrivenPropertyData?.Clear();
	}

	public void SetFloat(int nameID, float value, Mode mode = Mode.Auto)
	{
		if (m_NativePropertyBlock == null)
		{
			m_NativePropertyBlock = new MaterialPropertyBlock();
		}
		m_NativePropertyBlock.SetFloat(nameID, value);
		if (UseGPUDriven() && mode == Mode.Auto)
		{
			AddOrUpdateGPUDrivenProperty(GPUDrivenRenderer.PropertyData.Float(nameID, value));
		}
	}

	public void SetInteger(int nameID, int value, Mode mode = Mode.Auto)
	{
		if (m_NativePropertyBlock == null)
		{
			m_NativePropertyBlock = new MaterialPropertyBlock();
		}
		m_NativePropertyBlock.SetInteger(nameID, value);
		if (UseGPUDriven() && mode == Mode.Auto)
		{
			AddOrUpdateGPUDrivenProperty(GPUDrivenRenderer.PropertyData.Int(nameID, value));
		}
	}

	public void SetVector(int nameID, Vector4 value, Mode mode = Mode.Auto)
	{
		if (m_NativePropertyBlock == null)
		{
			m_NativePropertyBlock = new MaterialPropertyBlock();
		}
		m_NativePropertyBlock.SetVector(nameID, value);
		if (UseGPUDriven() && mode == Mode.Auto)
		{
			AddOrUpdateGPUDrivenProperty(GPUDrivenRenderer.PropertyData.Vector(nameID, value));
		}
	}

	public void SetColor(int nameID, Color value, Mode mode = Mode.Auto)
	{
		if (m_NativePropertyBlock == null)
		{
			m_NativePropertyBlock = new MaterialPropertyBlock();
		}
		m_NativePropertyBlock.SetColor(nameID, value);
		if (UseGPUDriven() && mode == Mode.Auto)
		{
			AddOrUpdateGPUDrivenProperty(GPUDrivenRenderer.PropertyData.Color(nameID, value.linear));
		}
	}

	public void SetMatrix(int nameID, Matrix4x4 value, Mode mode = Mode.Auto)
	{
		if (m_NativePropertyBlock == null)
		{
			m_NativePropertyBlock = new MaterialPropertyBlock();
		}
		m_NativePropertyBlock.SetMatrix(nameID, value);
		if (UseGPUDriven() && mode == Mode.Auto)
		{
			AddOrUpdateGPUDrivenProperty(GPUDrivenRenderer.PropertyData.Matrix(nameID, value));
		}
	}

	internal void Get([NotNull] Renderer renderer, [CanBeNull] ref GPUDrivenRenderer gpuDrivenRenderer, Mode mode)
	{
		if (renderer == null)
		{
			throw new ArgumentNullException("renderer");
		}
		if (UseGPUDriven(renderer) && mode == Mode.Auto)
		{
			if (m_GPUDrivenPropertyData == null)
			{
				m_GPUDrivenPropertyData = new List<GPUDrivenRenderer.PropertyData>();
			}
			m_GPUDrivenPropertyData.Clear();
			PopulateGPUDrivenRendererOrDefault(renderer, ref gpuDrivenRenderer);
			if (gpuDrivenRenderer != null)
			{
				ReadOnlySpan<GPUDrivenRenderer.PropertyData> instanceData = gpuDrivenRenderer.GetInstanceData();
				for (int i = 0; i < instanceData.Length; i++)
				{
					GPUDrivenRenderer.PropertyData item = instanceData[i];
					m_GPUDrivenPropertyData.Add(item);
				}
			}
		}
		else
		{
			if (m_NativePropertyBlock == null)
			{
				m_NativePropertyBlock = new MaterialPropertyBlock();
			}
			renderer.GetPropertyBlock(m_NativePropertyBlock);
		}
	}

	internal void Set([NotNull] Renderer renderer, [CanBeNull] ref GPUDrivenRenderer gpuDrivenRenderer, Mode mode)
	{
		if (renderer == null)
		{
			throw new ArgumentNullException("renderer");
		}
		if (UseGPUDriven(renderer) && mode == Mode.Auto)
		{
			EnsureGPUDrivenRenderer(renderer, ref gpuDrivenRenderer);
			gpuDrivenRenderer.SetPropertyData(m_GPUDrivenPropertyData);
			return;
		}
		if (m_NativePropertyBlock == null)
		{
			m_NativePropertyBlock = new MaterialPropertyBlock();
		}
		renderer.SetPropertyBlock(m_NativePropertyBlock);
	}

	private static void PopulateGPUDrivenRendererOrDefault(Renderer renderer, [CanBeNull] ref GPUDrivenRenderer gpuDrivenRenderer)
	{
		if (gpuDrivenRenderer == null && !renderer.TryGetComponent<GPUDrivenRenderer>(out gpuDrivenRenderer))
		{
			gpuDrivenRenderer = null;
		}
	}

	private static void EnsureGPUDrivenRenderer(Renderer renderer, ref GPUDrivenRenderer gpuDrivenRenderer)
	{
		if (gpuDrivenRenderer == null && !renderer.TryGetComponent<GPUDrivenRenderer>(out gpuDrivenRenderer))
		{
			gpuDrivenRenderer = renderer.gameObject.AddComponent<GPUDrivenRenderer>();
		}
	}

	private void AddOrUpdateGPUDrivenProperty(GPUDrivenRenderer.PropertyData propertyData)
	{
		if (m_GPUDrivenPropertyData == null)
		{
			m_GPUDrivenPropertyData = new List<GPUDrivenRenderer.PropertyData>();
		}
		bool flag = false;
		for (int i = 0; i < m_GPUDrivenPropertyData.Count; i++)
		{
			GPUDrivenRenderer.PropertyData value = m_GPUDrivenPropertyData[i];
			if (value.NameID == propertyData.NameID)
			{
				value.Value = propertyData.Value;
				m_GPUDrivenPropertyData[i] = value;
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			m_GPUDrivenPropertyData.Add(propertyData);
		}
	}
}
