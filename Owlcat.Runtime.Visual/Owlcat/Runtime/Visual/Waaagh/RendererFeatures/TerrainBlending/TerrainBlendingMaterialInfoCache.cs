using System.Collections.Generic;
using UnityEngine;

namespace Owlcat.Runtime.Visual.Waaagh.RendererFeatures.TerrainBlending;

internal sealed class TerrainBlendingMaterialInfoCache
{
	private readonly Dictionary<int, TerrainBlendingMaterialInfo> m_Infos = new Dictionary<int, TerrainBlendingMaterialInfo>();

	public bool TryGetMaterialInfo(Material terrainMaterial, out TerrainBlendingMaterialInfo result)
	{
		if (terrainMaterial == null)
		{
			result = default(TerrainBlendingMaterialInfo);
			return false;
		}
		Shader shader = terrainMaterial.shader;
		if (shader == null)
		{
			result = default(TerrainBlendingMaterialInfo);
			return false;
		}
		int instanceID = shader.GetInstanceID();
		if (!m_Infos.TryGetValue(instanceID, out result))
		{
			result = GetTerrainBlendingMaterialInfo(terrainMaterial);
			m_Infos.Add(instanceID, result);
		}
		return result.IsValid();
	}

	private static TerrainBlendingMaterialInfo GetTerrainBlendingMaterialInfo(Material material)
	{
		TerrainBlendingMaterialInfo result = default(TerrainBlendingMaterialInfo);
		result.BlendingDecalPass = material.FindPass("TerrainBlendingDecal");
		result.BlendingMaskPopulatePass = material.FindPass("TerrainBlendingMaskPopulate");
		result.BlendingMaskReducePass = material.FindPass("TerrainBlendingMaskReduce");
		result.Valid = result.BlendingDecalPass >= 0 && result.BlendingMaskReducePass >= 0 && result.BlendingMaskPopulatePass >= 0;
		if (!result.Valid)
		{
			return TerrainBlendingMaterialInfo.MakeInvalid();
		}
		return result;
	}
}
