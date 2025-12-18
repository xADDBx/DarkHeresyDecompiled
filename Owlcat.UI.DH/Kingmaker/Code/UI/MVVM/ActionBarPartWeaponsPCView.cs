using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Settings;
using Kingmaker.Settings.Entities;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ActionBarPartWeaponsPCView : View<ActionBarPartWeaponsVM>
{
	[SerializeField]
	private ActionBarWeaponSetPCView m_CurrentSet;

	[SerializeField]
	private OwlcatMultiButton m_ConvertButton;

	[Header("Hotkey block")]
	[SerializeField]
	private TextMeshProUGUI m_HotkeyText;

	[SerializeField]
	private GameObject m_HotkeyContainer;

	private string m_BindName;

	private SettingsEntityKeyBindingPair m_Binding;

	public void Initialize()
	{
		m_CurrentSet.Initialize(setKeyBindings: true);
		m_BindName = "ChangeWeaponSet";
		m_Binding = SettingsRoot.Controls.Keybindings.ActionBar.GetBindingPair("change-weapon-set");
		SetKeyBindLabel();
	}

	protected override void OnBind()
	{
		base.ViewModel.CurrentSet.Subscribe(m_CurrentSet.Bind).AddTo(this);
		base.ViewModel.CurrentSetIndex.Subscribe(SetConvertButtonState).AddTo(this);
		UISounds.Instance.SetClickAndHoverSound(m_ConvertButton, ButtonSoundsEnum.PlastickSound);
		base.ViewModel.CanSwitchSets.Subscribe(m_ConvertButton.SetInteractable).AddTo(this);
		m_ConvertButton.OnLeftClickAsObservable().Subscribe(base.ViewModel.ChangeWeaponSet).AddTo(this);
		m_ConvertButton.SetHint(UIStrings.Instance.ActionBar.ActionBarConvertWeapons).AddTo(this);
		Game.Instance.Keyboard.Bind(m_BindName, base.ViewModel.ChangeWeaponSet).AddTo(this);
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged += OnBindingChanged;
		}
	}

	protected override void OnUnbind()
	{
		m_CurrentSet.Unbind();
		if (m_Binding != null)
		{
			m_Binding.OnValueChanged -= OnBindingChanged;
		}
	}

	private void SetConvertButtonState(int index)
	{
		if (index != -1)
		{
			m_ConvertButton.SetActiveLayer(index);
		}
	}

	private void OnBindingChanged(KeyBindingPair obj)
	{
		SetKeyBindLabel();
	}

	private void SetKeyBindLabel()
	{
		string stringByBinding = UIKeyboardTexts.Instance.GetStringByBinding(Game.Instance.Keyboard.GetBindingByName(m_BindName));
		m_HotkeyText.text = stringByBinding;
		m_HotkeyContainer.SetActive(stringByBinding.Length > 0);
	}
}
