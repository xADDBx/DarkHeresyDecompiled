using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityBoolOnlyOneSaveConsoleView : SettingsEntityWithValueConsoleView<SettingsEntityBoolOnlyOneSaveVM>
{
	[SerializeField]
	private OwlcatMultiButton m_SelectableMultiButton;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.TempValue.Subscribe(SetValueFromSettings));
		SetToggleTexts();
	}

	protected override void UpdateLocalization()
	{
		base.UpdateLocalization();
		SetToggleTexts();
	}

	private void SetToggleTexts()
	{
		m_OnText.text = UIStrings.Instance.SettingsUI.SettingsToggleOn;
		m_OffText.text = UIStrings.Instance.SettingsUI.SettingsToggleOff;
	}

	private void SetValueFromSettings(bool value)
	{
		m_MultiSelectable.SetActiveLayer(value ? "On" : "Off");
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_SelectableMultiButton.SetFocus(value);
		m_SelectableMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
		base.OnModificationChanged(reason, allowed);
		m_MultiSelectable.Interactable = allowed;
	}

	public override bool HandleLeft()
	{
		if (base.ViewModel.ModificationAllowed.CurrentValue)
		{
			base.ViewModel.ChangeValue();
		}
		else
		{
			CallNotAllowedNotification();
		}
		return true;
	}

	public override bool HandleRight()
	{
		if (base.ViewModel.ModificationAllowed.CurrentValue)
		{
			base.ViewModel.ChangeValue();
		}
		else
		{
			CallNotAllowedNotification();
		}
		return true;
	}
}
