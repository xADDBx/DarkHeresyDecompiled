using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.VirtualTexture.Materials;

public static class ShaderMetadataExtractor
{
	private static readonly ShaderTagId s_VtSubShaderTag = new ShaderTagId(VirtualTextureUtils.UseOwlcatVT);

	public static ShaderMetadataRepository.ShaderMetadata CreateFrom(Shader shader)
	{
		ShaderMetadataRepository.ShaderMetadata result = default(ShaderMetadataRepository.ShaderMetadata);
		ExtractStackCountAndVtPropsId(shader, out result.TextureStackCount, out result.PropertyIdWidthVTAttribute, out result.PropertyNameIdWithVTAttribute);
		if (shader.FindSubshaderTagValue(0, s_VtSubShaderTag).name == "true")
		{
			result.HasVtShaderTag = true;
		}
		return result;
	}

	private static void ExtractStackCountAndVtPropsId(Shader shader, out int textureStackCount, out int[] vtPropsId, out int[] vtPropsNameId)
	{
		int propertyCount = shader.GetPropertyCount();
		int num = -1;
		ListPool<int>.Get(out var value);
		ListPool<int>.Get(out var value2);
		for (int i = 0; i < propertyCount; i++)
		{
			string[] propertyAttributes = shader.GetPropertyAttributes(i);
			for (int j = 0; j < propertyAttributes.Length; j++)
			{
				if (VirtualTextureUtils.TryParseVTAttribute(propertyAttributes[j], out var localStackId, out var _))
				{
					value2.Add(i);
					value.Add(shader.GetPropertyNameId(i));
					num = Math.Max(num, localStackId);
				}
			}
		}
		vtPropsId = value2.ToArray();
		vtPropsNameId = value.ToArray();
		ListPool<int>.Release(value);
		ListPool<int>.Release(value2);
		textureStackCount = num + 1;
	}
}
