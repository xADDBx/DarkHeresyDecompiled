using System;
using Kingmaker.Localization;
using UnityEngine;

namespace Kingmaker.Blueprints.Root.Strings;

[Serializable]
public class UIInspect
{
	[Header("Surface")]
	public LocalizedString Wounds;

	public LocalizedString Durability;

	public LocalizedString Defence;

	public LocalizedString DamageReduction;

	public LocalizedString MovePoints;

	public LocalizedString CharacterStatsTitle;

	public LocalizedString StatusEffectsTitle;

	public LocalizedString NoStatusEffects;

	public LocalizedString HasStatusEffects;

	[Obsolete("EffectsPositive")]
	public LocalizedString EffectsAlly;

	[Obsolete("EffectsNegative")]
	public LocalizedString EffectsEnemy;

	public LocalizedString EffectsPositive;

	public LocalizedString EffectsNegative;

	public LocalizedString EffectsDOT;

	public LocalizedString EffectsCritical;

	public LocalizedString WeaponsTitle;

	public LocalizedString AbilitiesTitle;

	public LocalizedString ActiveAbilitiesTitle;

	public LocalizedString ActiveAbilitiesShortTitle;

	public LocalizedString PassiveAbilitiesTitle;

	public LocalizedString PassiveAbilitiesShortTitle;

	public LocalizedString NoAbilities;

	public LocalizedString FeaturesTitle;

	public LocalizedString NoFeatures;

	public LocalizedString UltimateAbilitiesTitle;

	public LocalizedString ToggleSquad;

	public LocalizedString UnconditionalModifiers;
}
