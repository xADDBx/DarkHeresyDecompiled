using System.Collections.Generic;
using Owlcat.Runtime.Core.ObjectTracking;
using Unity.Collections;
using UnityEngine;

namespace Owlcat.Runtime.Visual.VirtualTexture.Materials;

public class MaterialTracker : ObjectTracker<Material>
{
	private VirtualTextureManager m_Manager;

	private List<Material> m_MaterialsToUpdate = new List<Material>();

	private List<int> m_MaterialsToRemove = new List<int>();

	public MaterialTracker(VirtualTextureManager manager, ObjectDispatcherService.TypeTrackingFlags typeTrackingFlags)
		: base(typeTrackingFlags)
	{
		m_Manager = manager;
	}

	public override void ProcessData(List<Object> changed, NativeArray<EntityId> changedID, NativeArray<EntityId> destroyedID)
	{
		m_MaterialsToUpdate.Clear();
		m_MaterialsToRemove.Clear();
		for (int i = 0; i < changed.Count; i++)
		{
			Material material = changed[i] as Material;
			if (material != null)
			{
				ProcessMaterialChanged(material);
			}
		}
		if (destroyedID.Length > 0)
		{
			foreach (EntityId item in destroyedID)
			{
				if (m_Manager.HasMaterial(item))
				{
					m_MaterialsToRemove.Add(item);
					m_Manager.MaterialMetadataRepository.InvalidateCache(item);
				}
			}
		}
		if (m_MaterialsToRemove.Count > 0)
		{
			m_Manager.ProcessDestroyedMaterials(m_MaterialsToRemove);
		}
		if (m_MaterialsToUpdate.Count > 0)
		{
			m_Manager.ProcessChangedMaterials(m_MaterialsToUpdate);
		}
	}

	private void ProcessMaterialChanged(Material material)
	{
		if (m_Manager.MaterialMetadataRepository.TryGet(material, out var metadata))
		{
			if (!MaterialHasChanges(material, in metadata))
			{
				return;
			}
			m_Manager.MaterialMetadataRepository.InvalidateCache(material);
			if (m_Manager.MaterialMetadataRepository.Get(material).SupportsVT)
			{
				m_MaterialsToUpdate.Add(material);
				return;
			}
			if (m_Manager.HasMaterial(material.GetInstanceID()))
			{
				m_MaterialsToRemove.Add(material.GetInstanceID());
			}
			ApplySentinelIfNeeded(material);
		}
		else
		{
			MaterialMetadataRepository.MaterialMetadata materialMetadata = m_Manager.MaterialMetadataRepository.Get(material);
			if (materialMetadata.SupportsVT && !m_Manager.HasMaterial(material.GetInstanceID()))
			{
				m_MaterialsToUpdate.Add(material);
			}
			else if (!materialMetadata.SupportsVT)
			{
				ApplySentinelIfNeeded(material);
			}
		}
	}

	private void ApplySentinelIfNeeded(Material material)
	{
		ShaderMetadataRepository.ShaderMetadata shaderMetadata = ShaderMetadataRepository.Instance.Get(material.shader);
		if (shaderMetadata.HasVtShaderTag && shaderMetadata.TextureStackCount == 0)
		{
			Debug.LogWarning("Material " + material.name + " (shader: " + material.shader.name + ") has UseOwlcatVT tag but zero TextureStack properties. VT disabled for this material.", material);
			VirtualTextureUtils.SetVTStackIndicesSentinel(material);
		}
	}

	private bool MaterialHasChanges(Material material, in MaterialMetadataRepository.MaterialMetadata metadata)
	{
		if (metadata.SupportsVT == VirtualTextureUtils.DoesMaterialUseVT(material) && metadata.TextureStackCount == VirtualTextureUtils.GetMaterialStackCount(material))
		{
			return MaterialTexturesChanged(material, in metadata);
		}
		return true;
	}

	private bool MaterialTexturesChanged(Material material, in MaterialMetadataRepository.MaterialMetadata metadata)
	{
		return false;
	}
}
