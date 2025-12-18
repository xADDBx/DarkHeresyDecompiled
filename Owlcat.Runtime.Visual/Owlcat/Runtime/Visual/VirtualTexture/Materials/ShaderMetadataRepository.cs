using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.Materials;

public class ShaderMetadataRepository
{
	public struct ShaderMetadata
	{
		public int TextureStackCount;

		public bool HasVtShaderTag;

		public int[] PropertyNameIdWithVTAttribute;

		public int[] PropertyIdWidthVTAttribute;
	}

	private Dictionary<Shader, ShaderMetadata> m_MetadataCache = new Dictionary<Shader, ShaderMetadata>();

	private static ShaderMetadataRepository s_Instance;

	public static ShaderMetadataRepository Instance
	{
		get
		{
			if (s_Instance == null)
			{
				s_Instance = new ShaderMetadataRepository();
			}
			return s_Instance;
		}
	}

	public ShaderMetadata Get(Shader shader)
	{
		if (m_MetadataCache.TryGetValue(shader, out var value))
		{
			return value;
		}
		return m_MetadataCache[shader] = ShaderMetadataExtractor.CreateFrom(shader);
	}

	internal void ProcessShaderChanges(Shader shader)
	{
	}
}
