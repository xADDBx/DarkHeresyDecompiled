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
		ExtractStackCountAndVtPropsId(shader, out result.TextureStackCount, out result.PropertyIdWidthVTAttribute, out result.PropertyNameIdWithVTAttribute, out result.LayerMasksPerStack);
		if (shader.FindSubshaderTagValue(0, s_VtSubShaderTag).name == "true")
		{
			result.HasVtShaderTag = true;
		}
		return result;
	}

	private static void ExtractStackCountAndVtPropsId(Shader shader, out int textureStackCount, out int[] vtPropsId, out int[] vtPropsNameId, out int[] layerMasksPerStack)
	{
		int propertyCount = shader.GetPropertyCount();
		int num = -1;
		Span<int> span = stackalloc int[16];
		span.Clear();
		ListPool<int>.Get(out var value);
		ListPool<int>.Get(out var value2);
		for (int i = 0; i < propertyCount; i++)
		{
			string[] propertyAttributes = shader.GetPropertyAttributes(i);
			for (int j = 0; j < propertyAttributes.Length; j++)
			{
				if (VirtualTextureUtils.TryParseVTAttribute(propertyAttributes[j], out var localStackId, out var layerIndex))
				{
					value2.Add(i);
					value.Add(shader.GetPropertyNameId(i));
					num = Math.Max(num, localStackId);
					if (localStackId >= 0 && localStackId < 16 && layerIndex >= 0 && layerIndex < 4)
					{
						span[localStackId] |= 1 << layerIndex;
					}
				}
			}
		}
		vtPropsId = value2.ToArray();
		vtPropsNameId = value.ToArray();
		ListPool<int>.Release(value);
		ListPool<int>.Release(value2);
		textureStackCount = num + 1;
		if (textureStackCount > 0)
		{
			layerMasksPerStack = new int[textureStackCount];
			int num2 = Math.Min(textureStackCount, 16);
			for (int k = 0; k < num2; k++)
			{
				layerMasksPerStack[k] = span[k];
			}
		}
		else
		{
			layerMasksPerStack = null;
		}
	}
}
