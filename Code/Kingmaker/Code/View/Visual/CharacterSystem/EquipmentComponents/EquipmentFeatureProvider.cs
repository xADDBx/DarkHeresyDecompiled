using Kingmaker.Blueprints;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

public abstract class EquipmentFeatureProvider
{
	private EquipmentFeatureFlag? m_FeatureFlags;

	public bool TryGetPhysicsDeformerLayout(out TriangleSkinmap triangleSkinmap, out GameObject prefabMesh)
	{
		triangleSkinmap = null;
		prefabMesh = null;
		BlueprintEquipmentFeatureReference[] featureList = GetFeatureList();
		if (featureList == null || featureList.Length == 0)
		{
			return false;
		}
		BlueprintEquipmentFeatureReference[] array = featureList;
		for (int i = 0; i < array.Length; i++)
		{
			EquipmentPhysicsFeatureComponent equipmentPhysicsFeatureComponent = array[i].Get()?.GetComponent<EquipmentPhysicsFeatureComponent>();
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
		if (!m_FeatureFlags.HasValue)
		{
			m_FeatureFlags = GetFeatureFlags();
			return m_FeatureFlags.Value.HasFlag(feature);
		}
		return m_FeatureFlags.Value.HasFlag(feature);
	}

	public bool IsHiddenByVisibilityFeatures(CharacterDisplayOptions displayOptions)
	{
		if (!displayOptions.ShowHelmet && HasFeature(EquipmentFeatureFlag.IsHiddenWithHelmet))
		{
			return true;
		}
		if (!displayOptions.ShowBackpack && HasFeature(EquipmentFeatureFlag.IsBackpack))
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

	protected abstract BlueprintEquipmentFeatureReference[] GetFeatureList();

	private EquipmentFeatureFlag GetFeatureFlags()
	{
		BlueprintEquipmentFeatureReference[] featureList = GetFeatureList();
		if (featureList == null || featureList.Length == 0)
		{
			return EquipmentFeatureFlag.None;
		}
		EquipmentFeatureFlag flags = EquipmentFeatureFlag.None;
		BlueprintEquipmentFeatureReference[] array = featureList;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Get()?.CallComponents(delegate(EquipmentFlagFeatureComponent f)
			{
				flags |= f.Feature;
			});
		}
		return flags;
	}
}
