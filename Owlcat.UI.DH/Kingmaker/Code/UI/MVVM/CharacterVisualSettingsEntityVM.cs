using System;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class CharacterVisualSettingsEntityVM : ViewModel
{
	private readonly ReactiveProperty<bool> m_IsOn = new ReactiveProperty<bool>();

	private readonly Action m_ChangeValue;

	private readonly ReactiveProperty<bool> m_Locked = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsOn => m_IsOn;

	public ReadOnlyReactiveProperty<bool> Locked => m_Locked;

	public CharacterVisualSettingsEntityVM(bool value, Action changeValue)
	{
		m_IsOn.Value = value;
		m_ChangeValue = changeValue;
	}

	public void Switch()
	{
		UISounds.Instance.Sounds.Buttons.ButtonHover.Play();
		m_ChangeValue?.Invoke();
		m_IsOn.Value = !IsOn.CurrentValue;
	}

	public void SetValue(bool value)
	{
		if (value != IsOn.CurrentValue)
		{
			Switch();
		}
	}

	public void SetLock(bool @lock = true)
	{
		m_Locked.Value = @lock;
	}
}
