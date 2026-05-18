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

	public UISettingsEntityEnemyDifficulty EnemyDurability;

	public UISettingsEntityEnemyDifficulty EnemyDamage;

	public UISettingsEntitySliderInt SkillCheckModifier;

	public UISettingsEntitySliderInt EnemyMovementPoints;

	public UISettingsEntitySliderInt EnemyDamageModifier;

	public UISettingsEntitySliderInt PartyDamageModifier;

	public UISettingsEntitySliderInt EnemyDodgeModifier;

	public UISettingsEntitySliderInt EnemySkillModifier;

	public UISettingsEntitySliderInt PartyPositiveMoraleChangeModifier;

	public UISettingsEntitySliderInt PartyNegativeMoraleChangeModifier;

	public UISettingsEntitySliderInt EnemyPositiveMoraleChangeModifier;

	public UISettingsEntitySliderInt EnemyNegativeMoraleChangeModifier;

	public void LinkToSettings()
	{
		GameDifficulty?.LinkSetting(SettingsRoot.Difficulty.GameDifficulty);
		OnlyOneSave?.LinkSetting(SettingsRoot.Difficulty.OnlyOneSave);
		CombatEncountersCapacity?.LinkSetting(SettingsRoot.Difficulty.CombatEncountersCapacity);
		RespecAllowed?.LinkSetting(SettingsRoot.Difficulty.RespecAllowed);
		EnemyDurability?.LinkSetting(SettingsRoot.Difficulty.EnemyDurability);
		EnemyDamage?.LinkSetting(SettingsRoot.Difficulty.EnemyDamage);
		SkillCheckModifier?.LinkSetting(SettingsRoot.Difficulty.SkillCheckModifier);
		EnemyMovementPoints?.LinkSetting(SettingsRoot.Difficulty.EnemyMovementPoints);
		EnemyDamageModifier?.LinkSetting(SettingsRoot.Difficulty.EnemyDamageModifier);
		PartyDamageModifier?.LinkSetting(SettingsRoot.Difficulty.PartyDamageModifier);
		EnemyDodgeModifier?.LinkSetting(SettingsRoot.Difficulty.EnemyDodgeModifier);
		EnemySkillModifier?.LinkSetting(SettingsRoot.Difficulty.EnemySkillModifier);
		PartyPositiveMoraleChangeModifier?.LinkSetting(SettingsRoot.Difficulty.PartyPositiveMoraleChangeModifier);
		PartyNegativeMoraleChangeModifier?.LinkSetting(SettingsRoot.Difficulty.PartyNegativeMoraleChangeModifier);
		EnemyPositiveMoraleChangeModifier?.LinkSetting(SettingsRoot.Difficulty.EnemyPositiveMoraleChangeModifier);
		EnemyNegativeMoraleChangeModifier?.LinkSetting(SettingsRoot.Difficulty.EnemyNegativeMoraleChangeModifier);
	}

	public void UpdateInteractable()
	{
		InitializeSettings();
	}

	public void InitializeSettings()
	{
		if (!(OnlyOneSave == null))
		{
			OnlyOneSave.ModificationAllowedCheck = () => GameUIState.Instance.IsInMainMenu || OnlyOneSave.Setting.GetValue();
			OnlyOneSave.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.GetLabelByOrigin(SettingsNotInteractableReasonType.OnlyOneSaveSwitchOn);
		}
	}
}
