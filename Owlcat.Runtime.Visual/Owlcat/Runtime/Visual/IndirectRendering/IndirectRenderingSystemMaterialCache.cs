using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.IndirectRendering;

public class IndirectRenderingSystemMaterialCache
{
	private readonly Dictionary<(IIndirectMesh mesh, Material material), Material> m_BRGMaterialCache = new Dictionary<(IIndirectMesh, Material), Material>();

	private readonly Dictionary<IIndirectMesh, List<Material>> m_BRGSourceMaterials = new Dictionary<IIndirectMesh, List<Material>>();

	public void ClearMeshMaterials(IIndirectMesh mesh)
	{
		if (!m_BRGSourceMaterials.Remove(mesh, out var value))
		{
			return;
		}
		foreach (Material item in value)
		{
			if (m_BRGMaterialCache.Remove((mesh, item), out var value2))
			{
				CoreUtils.Destroy(value2);
			}
		}
	}

	public void WriteSelectionHighlightColor(IIndirectMesh mesh, Color? color)
	{
	}

	public void SubstituteWithBRGMaterials(IIndirectMesh mesh, List<Material> materials)
	{
		for (int i = 0; i < materials.Count; i++)
		{
			Material material = materials[i];
			(IIndirectMesh, Material) key = (mesh, material);
			if (!m_BRGMaterialCache.TryGetValue(key, out var value))
			{
				value = new Material(material);
				value.name += "_IndirectRenderingSystem";
				m_BRGMaterialCache.Add(key, value);
				if (!m_BRGSourceMaterials.TryGetValue(mesh, out var value2))
				{
					value2 = new List<Material>();
					m_BRGSourceMaterials.Add(mesh, value2);
				}
				value2.Add(material);
			}
			materials[i] = value;
		}
	}

	public void Clear()
	{
		foreach (Material value in m_BRGMaterialCache.Values)
		{
			CoreUtils.Destroy(value);
		}
		m_BRGMaterialCache.Clear();
	}
}
