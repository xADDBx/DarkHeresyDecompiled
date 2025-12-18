using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;

[AllowedOn(typeof(BlueprintEquipmentFeature))]
[AllowMultipleComponents]
[TypeId("9eec65ca7d6b41c9944ba0652759ef8a")]
public class EquipmentFlagFeatureComponent : BlueprintComponent
{
	[SerializeField]
	private EquipmentFeatureFlag m_Feature;

	public EquipmentFeatureFlag Feature => m_Feature;
}
