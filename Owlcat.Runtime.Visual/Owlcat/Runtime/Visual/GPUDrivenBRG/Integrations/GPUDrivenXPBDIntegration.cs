using System.Collections.Generic;
using JetBrains.Annotations;
using Owlcat.Runtime.Visual.GPUDrivenBRG.ShaderMetadata;
using Owlcat.Runtime.Visual.XPBD.Shaders;
using UnityEngine;

namespace Owlcat.Runtime.Visual.GPUDrivenBRG.Integrations;

public static class GPUDrivenXPBDIntegration
{
	private const string kMesh = "XPBD_MESH";

	private const string kSkeleton = "XPBD_SKELETON";

	private const string kDeform = "XPBD_DEFORM";

	public const string kXPBDModeTag = "XPBD_MODE";

	internal static void AddProperties(Material material, List<GPUDrivenShaderMetadataRepository.ShaderPropertyData> collectedProperties)
	{
		string modeOrDefault = GetModeOrDefault(material);
		if (!string.IsNullOrWhiteSpace(modeOrDefault))
		{
			collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
			{
				PropertyIndex = -1,
				PerInstance = true,
				PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XpbdEnabledLocal, 0f)
			});
			switch (modeOrDefault)
			{
			case "XPBD_MESH":
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Matrix(ShaderPropertyId._XpbdBodyWorldToLocal, Matrix4x4.identity)
				});
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XpbdVertexToParticleMapOffset, 0f)
				});
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XpbdVertexOffset, 0f)
				});
				break;
			case "XPBD_SKELETON":
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XpbdBoneIndicesOffset, 0f)
				});
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XbdBonesOffset, 0f)
				});
				break;
			case "XPBD_DEFORM":
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XpbdVertexOffset, 0f)
				});
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XpbdDeformableVerticesOffset, 0f)
				});
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XpbdDeformableSkinnedVerticesOffset, 0f)
				});
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Float(ShaderPropertyId._XpbdVertexToDeformableVertexMapOffset, 0f)
				});
				collectedProperties.Add(new GPUDrivenShaderMetadataRepository.ShaderPropertyData
				{
					PropertyIndex = -1,
					PerInstance = true,
					PropertyData = GPUDrivenRenderer.PropertyData.Matrix(ShaderPropertyId._XpbdBodyWorldToLocal, Matrix4x4.identity)
				});
				break;
			}
		}
	}

	[CanBeNull]
	private static string GetModeOrDefault(Material material)
	{
		if (material.IsKeywordEnabled("XPBD_MESH"))
		{
			return "XPBD_MESH";
		}
		if (material.IsKeywordEnabled("XPBD_SKELETON"))
		{
			return "XPBD_SKELETON";
		}
		if (material.IsKeywordEnabled("XPBD_DEFORM"))
		{
			return "XPBD_DEFORM";
		}
		return material.GetTag("XPBD_MODE", searchFallbacks: false, null);
	}
}
