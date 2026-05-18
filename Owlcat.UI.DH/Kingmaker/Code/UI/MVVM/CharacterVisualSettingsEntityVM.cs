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
		ButtonsSounds.Instance.Default.Hover.Play();
		m_ChangeValue?.Invoke();
		m_IsOn.Value = !IsOn.CurrentValue;
	}

	public CharacterVisualSettingsEntityVM SetValue(bool value)
	{
		if (value != IsOn.CurrentValue)
		{
			Switch();
		}
		return this;
	}

	public CharacterVisualSettingsEntityVM SetLock(bool @lock = true)
	{
		m_Locked.Value = @lock;
		return this;
	}
}
