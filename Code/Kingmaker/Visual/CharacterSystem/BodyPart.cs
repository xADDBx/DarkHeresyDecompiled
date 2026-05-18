using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.View.Visual.CharacterSystem;
using Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
[KnowledgeDatabaseID("2aa814ffec855a34db72ef11aa489935")]
public class BodyPart : IEquipmentFeatureProvider
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

	private IEquipmentFeatureProvider m_EquipmentFeatureProvider;

	private IEquipmentFeatureProvider Features => m_EquipmentFeatureProvider ?? (m_EquipmentFeatureProvider = new EquipmentFeatureProvider(m_Features));

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

	public bool HasFeature(EquipmentFeatureFlag feature)
	{
		return Features.HasFeature(feature);
	}

	public bool TryGetPhysicsDeformerLayout(out TriangleSkinmap triangleSkinmap, out GameObject prefabMesh)
	{
		return Features.TryGetPhysicsDeformerLayout(out triangleSkinmap, out prefabMesh);
	}

	public bool IsHiddenByVisibilityFeatures(CharacterDisplayOptions displayOptions)
	{
		return Features.IsHiddenByVisibilityFeatures(displayOptions);
	}

	public IEnumerable<T> GetFeatureComponents<T>()
	{
		if (m_Features == null)
		{
			yield break;
		}
		BlueprintEquipmentFeatureReference[] features = m_Features;
		foreach (BlueprintEquipmentFeatureReference blueprintEquipmentFeatureReference in features)
		{
			foreach (T component in blueprintEquipmentFeatureReference.Get().GetComponents<T>())
			{
				yield return component;
			}
		}
	}
}
