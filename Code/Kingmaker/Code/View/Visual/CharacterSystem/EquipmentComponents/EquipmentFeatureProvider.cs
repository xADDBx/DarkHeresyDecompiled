using System.Collections.Generic;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

public class EquipmentFeatureProvider : IEquipmentFeatureProvider
{
	private EquipmentFeatureFlag m_FeatureFlags;

	private BlueprintEquipmentFeatureReference[] m_Features;

	private bool m_HasFeatures;

	public EquipmentFeatureProvider(BlueprintEquipmentFeatureReference[] features)
	{
		m_Features = features;
		m_HasFeatures = features != null && features.Length > 0;
		m_FeatureFlags = EquipmentFeatureFlag.None;
		if (!m_HasFeatures)
		{
			return;
		}
		BlueprintEquipmentFeatureReference[] features2 = m_Features;
		for (int i = 0; i < features2.Length; i++)
		{
			features2[i].Get()?.CallComponents(delegate(EquipmentFlagFeatureComponent f)
			{
				m_FeatureFlags |= f.Feature;
			});
		}
	}

	public bool TryGetPhysicsDeformerLayout(out TriangleSkinmap triangleSkinmap, out GameObject prefabMesh)
	{
		triangleSkinmap = null;
		prefabMesh = null;
		if (!m_HasFeatures)
		{
			return false;
		}
		BlueprintEquipmentFeatureReference[] features = m_Features;
		for (int i = 0; i < features.Length; i++)
		{
			EquipmentPhysicsFeatureComponent equipmentPhysicsFeatureComponent = features[i].Get()?.GetComponent<EquipmentPhysicsFeatureComponent>();
			if (equipmentPhysicsFeatureComponent != null)
			{
				triangleSkinmap = equipmentPhysicsFeatureComponent.TriangleSkinmap;
				prefabMesh = equipmentPhysicsFeatureComponent.MasterPrefab;
				return true;
			}
		}
		return false;
	}

	public bool HasFeature(EquipmentFeatureFlag feature)
	{
		return m_FeatureFlags.HasFlag(feature);
	}

	public bool IsHiddenByVisibilityFeatures(CharacterDisplayOptions displayOptions)
	{
		if (!displayOptions.ShowHelmet && HasFeature(EquipmentFeatureFlag.IsHelmet))
		{
			return true;
		}
		if (!displayOptions.ShowArmor && HasFeature(EquipmentFeatureFlag.IsArmor))
		{
			return true;
		}
		if (!displayOptions.ShowBackpack && HasFeature(EquipmentFeatureFlag.IsBackpack))
		{
			return true;
		}
		if (!displayOptions.ShowCloak && HasFeature(EquipmentFeatureFlag.IsCloak))
		{
			return true;
		}
		if (!displayOptions.ShowGloves && HasFeature(EquipmentFeatureFlag.IsGloves))
		{
			return true;
		}
		if (!displayOptions.ShowBoots && HasFeature(EquipmentFeatureFlag.IsBoots))
		{
			return true;
		}
		if (displayOptions.IsPeacefulMode && HasFeature(EquipmentFeatureFlag.IsHiddenInPeacefulMode))
		{
			return true;
		}
		if (!displayOptions.IsInDollRoom && HasFeature(EquipmentFeatureFlag.IsVisibleOnlyInDollRoom))
		{
			return true;
		}
		return false;
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
			foreach (T component in blueprintEquipmentFeatureReference.Blueprint.GetComponents<T>())
			{
				yield return component;
			}
		}
	}
}
