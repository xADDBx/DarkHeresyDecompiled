using System;
using System.Text.RegularExpressions;
using Owlcat.Runtime.Visual.VirtualTexture.Materials;
using Owlcat.Runtime.Visual.VirtualTexture.TiledTexture;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture;

public static class VirtualTextureUtils
{
	public static readonly string UseOwlcatVT = "UseOwlcatVT";

	public static readonly string StackIdLayerIdFormat = "VT_STACK_{0}_LAYER_{1}";

	public static readonly string VT_ENABLED = "VT_ENABLED";

	public static readonly Matrix4x4 VTStackIndicesSentinel = CreateSentinelMatrix();

	public static void SetMaterialStackLayerTag(Material material, int localStackId, int layerIndex, string guid)
	{
		material.SetOverrideTag(string.Format(StackIdLayerIdFormat, localStackId, layerIndex), guid);
	}

	public static bool TryParseVTAttribute(string attribute, out int localStackId, out int layerIndex)
	{
		localStackId = -1;
		layerIndex = -1;
		Match match = Regex.Match(attribute, "VirtualTexture\\((\\d+),\\s*(\\d+)(?:,\\s*[^)]+)?\\)");
		if (match.Success)
		{
			localStackId = int.Parse(match.Groups[1].Value);
			layerIndex = int.Parse(match.Groups[2].Value);
			return true;
		}
		return false;
	}

	internal static int GetMaterialStackCount(Material material)
	{
		return ShaderMetadataRepository.Instance.Get(material.shader).TextureStackCount;
	}

	internal static bool DoesMaterialUseVT(Material material)
	{
		ShaderMetadataRepository.ShaderMetadata shaderMetadata = ShaderMetadataRepository.Instance.Get(material.shader);
		if (shaderMetadata.HasVtShaderTag)
		{
			return shaderMetadata.TextureStackCount > 0;
		}
		return material.IsKeywordEnabled(VT_ENABLED);
	}

	private static Matrix4x4 CreateSentinelMatrix()
	{
		Matrix4x4 result = default(Matrix4x4);
		for (int i = 0; i < 16; i++)
		{
			result[i] = -1f;
		}
		return result;
	}

	internal static void SetVTStackIndicesSentinel(Material material)
	{
		material.SetMatrix(ShaderPropertyId._VTStackIndices, VTStackIndicesSentinel);
	}

	internal static bool ValidateTextureStackId(ref TextureStackId stackId)
	{
		bool result = true;
		if (stackId.Layer0 != Guid.Empty && !TiledTextureDB.HasTiles(in stackId.Layer0))
		{
			result = false;
		}
		if (stackId.Layer1 != Guid.Empty && !TiledTextureDB.HasTiles(in stackId.Layer1))
		{
			result = false;
		}
		if (stackId.Layer2 != Guid.Empty && !TiledTextureDB.HasTiles(in stackId.Layer2))
		{
			result = false;
		}
		if (stackId.Layer3 != Guid.Empty && !TiledTextureDB.HasTiles(in stackId.Layer3))
		{
			result = false;
		}
		return result;
	}

	public static bool AllItemsAreEqual<T>(T[] array1, T[] array2) where T : struct, IEquatable<T>
	{
		if (array1 == null || array2 == null)
		{
			return array1 == array2;
		}
		if (array1.Length != array2.Length)
		{
			return false;
		}
		for (int i = 0; i < array1.Length; i++)
		{
			if (!array1[i].Equals(array2[i]))
			{
				return false;
			}
		}
		return true;
	}

	public static int CalculateTiledMipCount(Texture2D texture)
	{
		return CalculateTiledMipCount(new int2(texture.width, texture.height));
	}

	public static int CalculateTiledMipCount(int2 srcMipSize)
	{
		int num = 0;
		while (math.all(srcMipSize >= 128))
		{
			num++;
			srcMipSize >>= 1;
		}
		return num;
	}
}
