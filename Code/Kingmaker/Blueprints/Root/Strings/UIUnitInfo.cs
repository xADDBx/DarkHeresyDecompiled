using System;
using Kingmaker.Localization;
using UnityEngine.Serialization;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIUnitInfo
{
	public LocalizedString DamageModifiers;

	public LocalizedString VitalDamageLockedByArmor;

	public LocalizedString VitalDamageLockedByArmorHint;

	public LocalizedString VitalDamageLockedByStrategy;

	public LocalizedString VitalDamageLockedByStrategyHint;

	public LocalizedString CriticalEffectsLockedByArmor;

	public LocalizedString CriticalEffectsLockedByArmorHint;

	public LocalizedString CriticalEffectsThroughArmor;

	public LocalizedString CriticalEffectsThroughArmorHint;

	[FormerlySerializedAs("Concentration")]
	public LocalizedString ConcentrationTitle;
}
