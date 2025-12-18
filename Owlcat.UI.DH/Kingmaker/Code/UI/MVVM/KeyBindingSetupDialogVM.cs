using System;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Settings.Entities;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class KeyBindingSetupDialogVM : ViewModel
{
	private readonly UISettingsEntityKeyBinding m_UISettingsEntity;

	private readonly int m_BindingIndex;

	private readonly Action m_CloseAction;

	public KeyBindingData CurrentKeyBinding { get; private set; }

	public bool CurrentBindingIsOccupied { get; private set; }

	public KeyBindingSetupDialogVM(UISettingsEntityKeyBinding uiSettingsEntity, int bindingIndex, Action closeAction)
	{
		m_UISettingsEntity = uiSettingsEntity;
		m_BindingIndex = bindingIndex;
		m_CloseAction = closeAction;
		CurrentKeyBinding = uiSettingsEntity.GetBinding(bindingIndex);
	}

	public void OnBindingChosen(KeyBindingData keyBindingData)
	{
		CurrentKeyBinding = keyBindingData;
		CurrentBindingIsOccupied = !m_UISettingsEntity.TrySetBinding(keyBindingData, m_BindingIndex);
		if (!CurrentBindingIsOccupied)
		{
			Close();
		}
	}

	public void Unbind()
	{
		m_UISettingsEntity.TrySetBinding(new KeyBindingData
		{
			Key = KeyCode.None
		}, m_BindingIndex);
		Close();
	}

	public void Close()
	{
		m_CloseAction?.Invoke();
	}
}
