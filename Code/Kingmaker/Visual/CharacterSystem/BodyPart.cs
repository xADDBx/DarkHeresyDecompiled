using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;
using Kingmaker.ElementsSystem;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
[KnowledgeDatabaseID("2aa814ffec855a34db72ef11aa489935")]
public class BodyPart : EquipmentFeatureProvider
{
	[KDB("Тип боди парта")]
	[SerializeField]
	private BodyPartType m_Type;

	[DrawPreview]
	[KDB("Вьюха для боди порта. Должна иметь хотя бы один SkinnedMeshRenderer. Опционально")]
	[SerializeField]
	private GameObject m_Model;

	[KDB("Набор текстур для перекраски. Diffuse текстура обязательна")]
	[SerializeField]
	private CharacterTextureDescription m_Textures;

	[KDB("Геймплейные особенности предмета")]
	[SerializeField]
	private BlueprintEquipmentFeatureReference[] m_Features;

	private SkinnedMeshRenderer[] m_SkinnedRenderer;

	public GameObject Model => m_Model;

	public CharacterTextureDescription Textures => m_Textures;

	public BodyPartType Type
	{
		get
		{
			return m_Type;
		}
		set
		{
			m_Type = value;
		}
	}

	public SkinnedMeshRenderer[] SkinnedRenderers
	{
		get
		{
			if (Model == null)
			{
				return null;
			}
			SkinnedMeshRenderer[] skinnedRenderer = m_SkinnedRenderer;
			if (skinnedRenderer != null && skinnedRenderer.Length != 0)
			{
				return m_SkinnedRenderer;
			}
			m_SkinnedRenderer = Model.GetComponentsInChildren<SkinnedMeshRenderer>();
			return m_SkinnedRenderer;
		}
	}

	public BodyPart(BodyPartType bpType, GameObject model, CharacterTextureDescription textures)
	{
		m_Type = bpType;
		m_Model = model;
		m_Textures = textures;
	}

	public void SetModel(GameObject model)
	{
		m_Model = model;
	}

	public bool HasMesh()
	{
		SkinnedMeshRenderer[] skinnedRenderers = SkinnedRenderers;
		if (skinnedRenderers == null)
		{
			return false;
		}
		return skinnedRenderers.Length != 0;
	}

	protected override BlueprintEquipmentFeatureReference[] GetFeatureList()
	{
		return m_Features;
	}
}
