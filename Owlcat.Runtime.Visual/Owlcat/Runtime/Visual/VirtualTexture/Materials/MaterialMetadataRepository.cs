using System;
using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.Materials;

public class MaterialMetadataRepository : IDisposable
{
	public struct MaterialMetadata
	{
		public bool SupportsVT;

		public int TextureStackCount;
	}

	private Dictionary<int, MaterialMetadata> m_MetadataCache = new Dictionary<int, MaterialMetadata>();

	public void Dispose()
	{
	}

	internal bool TryGet(Material material, out MaterialMetadata metadata)
	{
		return m_MetadataCache.TryGetValue(material.GetInstanceID(), out metadata);
	}

	internal void InvalidateCache(Material material)
	{
		m_MetadataCache.Remove(material.GetInstanceID());
	}

	internal void InvalidateCache(int materialId)
	{
		m_MetadataCache.Remove(materialId);
	}

	public MaterialMetadata Get(Material material)
	{
		if (m_MetadataCache.TryGetValue(material.GetInstanceID(), out var value))
		{
			return value;
		}
		return m_MetadataCache[material.GetInstanceID()] = MaterialMetadataExtractor.GetFrom(material);
	}
}
