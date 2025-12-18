using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsEntityBoolOnlyOneSavePCView : SettingsEntityWithValueView<SettingsEntityBoolOnlyOneSaveVM>
{
	[SerializeField]
	private OwlcatMultiButton m_MultiButton;

	[SerializeField]
	private TextMeshProUGUI m_OnText;

	[SerializeField]
	private TextMeshProUGUI m_OffText;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.TempValue.Subscribe(SetValueFromSettings));
		AddDisposable(m_MultiButton.OnLeftClickAsObservable().Subscribe(SwitchValue));
		SubscribeNotAllowedSelectable(m_MultiButton);
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

	private void SwitchValue()
	{
		base.ViewModel.ChangeValue();
	}

	private void SetValueFromSettings(bool value)
	{
		m_MultiButton.SetActiveLayer(value ? "On" : "Off");
	}

	public override void OnModificationChanged(string reason, bool allowed = true)
	{
		base.OnModificationChanged(reason, allowed);
		m_MultiButton.Interactable = allowed;
		SetNotAllowedModificationHint(m_MultiButton);
	}

	public override bool HandleLeft()
	{
		if (base.ViewModel.ModificationAllowed.CurrentValue)
		{
			base.ViewModel.ChangeValue();
		}
		return true;
	}

	public override bool HandleRight()
	{
		if (base.ViewModel.ModificationAllowed.CurrentValue)
		{
			base.ViewModel.ChangeValue();
		}
		return true;
	}
}
