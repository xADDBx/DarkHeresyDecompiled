using System.Collections.Generic;
using Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;
using Owlcat.Runtime.Visual.VirtualTexture;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations;

public static class GPUDrivenVirtualTextureIntegration
{
	internal static void AddProperties(Material material, List<GPUDrivenShaderMetadataRepository.ShaderPropertyData> collectedProperties)
	{
		if (VirtualTextureUtils.DoesMaterialUseVT(material))
		{
			collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
			{
				PropertyIndex = -1,
				PerInstance = false,
				PropertyData = GPUDrivenRenderer.PropertyData.Matrix(ShaderPropertyId._VTStackIndices, Matrix4x4.zero)
			});
		}
	}
}
