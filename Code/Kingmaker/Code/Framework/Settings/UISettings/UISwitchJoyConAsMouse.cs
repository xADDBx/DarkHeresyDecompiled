using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Settings;
using Kingmaker.Tutorial;
using Owlcat.QA.Validation;
using UnityEngine;

namespace Kingmaker.Code.Framework.Settings.UISettings;

[Serializable]
public class UISwitchJoyConAsMouse : IUISettingsSheet
{
	public UISettingsEntityBool JoyConActivate;

	[SerializeField]
	[ValidateNotNull]
	public BlueprintTutorial.Reference JoyConDeattachTutorial;

	public void LinkToSettings()
	{
		JoyConActivate.LinkSetting(SettingsRoot.Game.Switch.SwitchJoyConAsMouse);
		SettingsRoot.Game.Switch.JoyConDeattachTutorial = JoyConDeattachTutorial;
	}

	public void InitializeSettings()
	{
	}

	public void UpdateInteractable()
	{
		JoyConActivate.ModificationAllowedCheck = () => Game.Instance.RootUIContext.CanChangeInput;
		JoyConActivate.ModificationAllowedReason = UIStrings.Instance.InteractableSettingsReasons.CannotSwitchJoyConInputBecause;
	}
}
