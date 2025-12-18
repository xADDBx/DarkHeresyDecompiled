using System;
using System.Linq;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.Attributes;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.UnitLogic.FactLogic;

[Serializable]
[TypeId("16aa52a700244b00b8648b54c979e25a")]
[ComponentName("CriticalEffects/OverrideUnitBodyWithRandomHandsSettings")]
[AllowedOn(typeof(BlueprintUnit))]
public class OverrideUnitBodyWithRandomHandsSettings : UnitFactComponentDelegate
{
	public UnitItemEquipmentHandSettingsWithWeights[] SettingsWithWeights;

	public float TotalWeightPercent => SettingsWithWeights.Sum((UnitItemEquipmentHandSettingsWithWeights e) => e.Weight);
}
