using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Pool;

namespace Kingmaker.Visual.MaterialEffects.LayeredMaterial;

internal sealed class LayeredMaterialRenderer
{
	private const int kBaseMaterialsCount = 1;

	private static readonly Stack<LayeredMaterialRenderer> s_Pool = new Stack<LayeredMaterialRenderer>();

	private readonly MaterialPropertiesSnapshot m_BaseMaterialPropertiesSnapshot = new MaterialPropertiesSnapshot();

	private Renderer m_Renderer;

	public static LayeredMaterialRenderer Get([NotNull] Renderer renderer)
	{
		if (renderer == null)
		{
			throw new ArgumentNullException("renderer");
		}
		if (!s_Pool.TryPop(out var result))
		{
			result = new LayeredMaterialRenderer();
		}
		result.m_Renderer = renderer;
		result.RefreshBaseMaterialPropertiesSnapshot();
		return result;
	}

	public void Recycle()
	{
		m_Renderer = null;
		m_BaseMaterialPropertiesSnapshot.Clear();
	}

	public bool IsValid()
	{
		return m_Renderer != null;
	}

	public bool IsRendererIdMatch(int rendererId)
	{
		if (IsValid())
		{
			return m_Renderer.GetInstanceID() == rendererId;
		}
		return false;
	}

	public MaterialPropertiesSnapshot GetBaseMaterialPropertiesSnapshot()
	{
		return m_BaseMaterialPropertiesSnapshot;
	}

	public void RefreshBaseMaterialPropertiesSnapshot()
	{
		if (m_Renderer == null)
		{
			throw new NullReferenceException("m_Renderer");
		}
		m_BaseMaterialPropertiesSnapshot.Capture(m_Renderer.sharedMaterial);
	}

	public void SetMaterials([CanBeNull] List<Material> additionalMaterials)
	{
		if (m_Renderer == null)
		{
			throw new NullReferenceException("m_Renderer");
		}
		List<Material> value;
		using (CollectionPool<List<Material>, Material>.Get(out value))
		{
			m_Renderer.GetSharedMaterials(value);
			if (value.Count < 1)
			{
				return;
			}
			int i = 1;
			for (int count = value.Count; i < count; i++)
			{
				m_Renderer.SetPropertyBlock(null, i);
			}
			if (additionalMaterials != null && additionalMaterials.Count > 0)
			{
				int num = 1 + additionalMaterials.Count;
				if (value.Count > num)
				{
					value.RemoveRange(num, value.Count - num);
				}
				else if (value.Count < num)
				{
					int num2 = num - value.Count;
					for (int j = 0; j < num2; j++)
					{
						value.Add(null);
					}
				}
				int k = 0;
				for (int count2 = additionalMaterials.Count; k < count2; k++)
				{
					value[1 + k] = additionalMaterials[k];
				}
			}
			else
			{
				value.RemoveRange(1, value.Count - 1);
			}
			m_Renderer.SetSharedMaterialsExt(value);
		}
	}

	public void SetMaterialPropertyBlock(MaterialPropertyBlock materialPropertyBlock, int layerIndex)
	{
		if (m_Renderer == null)
		{
			throw new NullReferenceException("m_Renderer");
		}
		int num = 1 + layerIndex;
		List<Material> value;
		using (CollectionPool<List<Material>, Material>.Get(out value))
		{
			m_Renderer.GetSharedMaterials(value);
			if (num >= value.Count)
			{
				return;
			}
		}
		m_Renderer.SetPropertyBlock(materialPropertyBlock, num);
	}
}
