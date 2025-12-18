using System;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.Utility.UniRxExtensions;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityBoolOnlyOneSaveVM : SettingsEntityWithValueVM
{
	private readonly UISettingsEntityBoolOnlyOneSave m_UISettingsEntity;

	public readonly ReadOnlyReactiveProperty<bool> TempValue;

	public SettingsEntityBoolOnlyOneSaveVM(UISettingsEntityBoolOnlyOneSave uiSettingsEntity, bool hideMarkImage = false)
		: base(uiSettingsEntity, hideMarkImage)
	{
		m_UISettingsEntity = uiSettingsEntity;
		AddDisposable(TempValue = m_UISettingsEntity.Setting.ObserveTempValue());
	}

	private bool GetTempValue()
	{
		return m_UISettingsEntity.Setting.GetTempValue();
	}

	private void SetTempValue(bool value)
	{
		if (base.ModificationAllowed.CurrentValue)
		{
			m_UISettingsEntity.Setting.SetTempValue(value);
		}
	}

	public void ChangeValue()
	{
		bool currentValue = GetTempValue();
		string text = ((!GameUIState.Instance.IsInMainMenu) ? (currentValue ? ((string)UIStrings.Instance.SettingsUI.AreYouSureSwitchOffGrimDarkness) : string.Empty) : ((!currentValue) ? (UIStrings.Instance.SettingsUI.AreYouSureSwitchOnGrimDarkness.Text + Environment.NewLine + Environment.NewLine + UIStrings.Instance.SettingsUI.GrimDaknessSettingsWarning.Text) : string.Empty));
		if (!string.IsNullOrWhiteSpace(text))
		{
			UtilityMessageBox.ShowMessageBox(text, DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton button)
			{
				if (button == DialogMessageBoxButton.Yes)
				{
					SetTempValue(!currentValue);
				}
			});
		}
		else
		{
			SetTempValue(!currentValue);
		}
	}
}
