using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.Materials;

public static class MaterialMetadataExtractor
{
	public static MaterialMetadataRepository.MaterialMetadata GetFrom(Material material)
	{
		ShaderMetadataRepository.ShaderMetadata shaderMetadata = ShaderMetadataRepository.Instance.Get(material.shader);
		MaterialMetadataRepository.MaterialMetadata result = default(MaterialMetadataRepository.MaterialMetadata);
		result.SupportsVT = VirtualTextureUtils.DoesMaterialUseVT(material);
		result.TextureStackCount = shaderMetadata.TextureStackCount;
		return result;
	}
}
