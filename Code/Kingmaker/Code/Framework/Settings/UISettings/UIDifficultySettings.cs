using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Settings;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UIDifficultySettings : IUISettingsSheet
{
	public UISettingsEntityGameDifficulty GameDifficulty;

	public UISettingsEntityBoolOnlyOneSave OnlyOneSave;

	public UISettingsEntityDropdownCombatEncountersCapacity CombatEncountersCapacity;

	public UISettingsEntityBool RespecAllowed;

	public UISettingsEntitySliderInt EnemyDodgePercentModifier;

	public UISettingsEntitySliderInt MinPartyDamage;

	public UISettingsEntitySliderInt MinPartyDamageFraction;

	public UISettingsEntitySliderInt NPCAttributesBaseValuePercentMultiplier;

	public UISettingDropdownHardCrowdControlDurationLimit HardCrowdControlOnPartyMaxDurationRounds;

	public UISettingsEntitySliderInt SkillCheckModifier;

	public UISettingsEntitySliderInt EnemyHitPointsPercentModifier;

	public UISettingsEntitySliderInt PartyDamageDealtAfterArmorReductionPercentModifier;

	public UISettingsEntitySliderInt AvoidableDamagePercentModifier;

	public UISettingsEntitySliderInt EnemyMovementPoints;

	public UISettingsEntityNPCDifficulty NPCDifficulty;

	public void LinkToSettings()
	{
		GameDifficulty.LinkSetting(SettingsRoot.Difficulty.GameDifficulty);
		OnlyOneSave.LinkSetting(SettingsRoot.Difficulty.OnlyOneSave);
		CombatEncountersCapacity.LinkSetting(SettingsRoot.Difficulty.CombatEncountersCapacity);
		RespecAllowed.LinkSetting(SettingsRoot.Difficulty.RespecAllowed);
		EnemyDodgePercentModifier.LinkSetting(SettingsRoot.Difficulty.EnemyDodgePercentModifier);
		MinPartyDamage.LinkSetting(SettingsRoot.Difficulty.MinPartyDamage);
		MinPartyDamageFraction.LinkSetting(SettingsRoot.Difficulty.MinPartyDamageFraction);
		NPCAttributesBaseValuePercentMultiplier.LinkSetting(SettingsRoot.Difficulty.NPCAttributesBaseValuePercentModifier);
		HardCrowdControlOnPartyMaxDurationRounds.LinkSetting(SettingsRoot.Difficulty.HardCrowdControlOnPartyMaxDurationRounds);
		SkillCheckModifier.LinkSetting(SettingsRoot.Difficulty.SkillCheckModifier);
		EnemyHitPointsPercentModifier.LinkSetting(SettingsRoot.Difficulty.EnemyHitPointsPercentModifier);
		PartyDamageDealtAfterArmorReductionPercentModifier.LinkSetting(SettingsRoot.Difficulty.PartyDamageDealtAfterArmorReductionPercentModifier);
		AvoidableDamagePercentModifier.LinkSetting(SettingsRoot.Difficulty.AvoidableDamagePercentModifier);
		EnemyMovementPoints.LinkSetting(SettingsRoot.Difficulty.EnemyMovementPoints);
		NPCDifficulty.LinkSetting(SettingsRoot.Difficulty.NPCDifficulty);
	}

	public void UpdateInteractable()
	{
		InitializeSettings();
	}

	public void InitializeSettings()
	{
		OnlyOneSave.ModificationAllowedCheck = () => GameUIState.Instance.IsInMainMenu || OnlyOneSave.Setting.GetValue();
		OnlyOneSave.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.GetLabelByOrigin(SettingsNotInteractableReasonType.OnlyOneSaveSwitchOn);
	}
}
