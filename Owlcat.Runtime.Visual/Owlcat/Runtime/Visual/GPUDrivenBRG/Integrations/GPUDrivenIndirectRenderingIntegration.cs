using System.Collections.Generic;
using Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;
using Owlcat.Runtime.Visual.IndirectRendering;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations;

public static class GPUDrivenIndirectRenderingIntegration
{
	internal static void AddProperties(Material material, List<GPUDrivenShaderMetadataRepository.ShaderPropertyData> collectedProperties)
	{
		if (IndirectRenderingSystem.ValidateMaterialCompatibility(material, out var _))
		{
			collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
			{
				PropertyIndex = -1,
				PerInstance = true,
				PropertyData = GPUDrivenRenderer.PropertyData.Vector(ShaderPropertyId._IndirectInstanceData_BakedGI, Vector4.zero)
			});
			collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
			{
				PropertyIndex = -1,
				PerInstance = true,
				PropertyData = GPUDrivenRenderer.PropertyData.Vector(ShaderPropertyId._IndirectInstanceData_Shadowmask, Vector4.zero)
			});
			collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
			{
				PropertyIndex = -1,
				PerInstance = true,
				PropertyData = GPUDrivenRenderer.PropertyData.Vector(ShaderPropertyId._IndirectInstanceData_TintColor, Vector4.zero)
			});
		}
	}
}
