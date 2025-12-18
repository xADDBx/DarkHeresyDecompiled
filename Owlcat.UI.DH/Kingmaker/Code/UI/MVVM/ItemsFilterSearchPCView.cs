using Kingmaker.Code.Framework.Settings.UISettings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ItemsFilterSearchPCView : ItemsFilterSearchBaseView
{
	[Header("Dropdown Part")]
	[SerializeField]
	private OwlcatButton m_DropdownButton;

	[SerializeField]
	private TMP_Dropdown m_Dropdown;

	protected override void OnBind()
	{
		base.OnBind();
		m_InputField.text = string.Empty;
		Observable.EveryValueChanged(m_InputField, (TMP_InputField f) => f.text).Skip(1).Subscribe(base.OnSearchStringEdit)
			.AddTo(this);
		if ((bool)m_DropdownButton && (bool)m_Dropdown)
		{
			ObservableSubscribeExtensions.Subscribe(m_DropdownButton.OnLeftClickAsObservable(), delegate
			{
				ShowDropdown();
			}).AddTo(this);
			m_Dropdown.onValueChanged.AsObservable().Subscribe(SetValueFromDropdown).AddTo(this);
			SetupDropdown();
		}
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.OpenSearchInventory.name, ActivateDeactivateInputField).AddTo(this);
	}

	public void ActivateDeactivateInputField()
	{
		if (m_InputField.isFocused)
		{
			m_InputField.DeactivateInputField();
		}
		else
		{
			m_InputField.ActivateInputField();
		}
	}

	private void SetValueFromDropdown(int value)
	{
		m_InputField.text = DropdownValues[value];
	}

	public void ShowDropdown()
	{
		if (m_Dropdown.IsExpanded)
		{
			m_Dropdown.Hide();
		}
		else
		{
			m_Dropdown.Show();
		}
	}

	private void SetupDropdown()
	{
		m_Dropdown.ClearOptions();
		m_Dropdown.AddOptions(DropdownValues);
	}

	public override void SetActive(bool value)
	{
		base.gameObject.SetActive(value);
		m_Dropdown.Hide();
		m_InputField.text = string.Empty;
		if (value)
		{
			m_InputField.Select();
		}
	}
}
