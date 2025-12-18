using System;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.UnitLogic.Progression.Features;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace Kingmaker.Designers.Mechanics.EquipmentEnchants;

[Obsolete]
[AllowMultipleComponents]
[TypeId("01f56971790b338448973ff85009d309")]
public class AddUnitFeatureEquipment : BlueprintComponent
{
	[SerializeField]
	[FormerlySerializedAs("Feature")]
	private BlueprintFeatureReference m_Feature;

	public BlueprintFeature Feature => m_Feature?.Get();
}
