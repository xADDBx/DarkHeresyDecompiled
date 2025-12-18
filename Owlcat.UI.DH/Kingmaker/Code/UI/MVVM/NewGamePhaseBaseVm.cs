using System;
using Kingmaker.Settings;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public abstract class NewGamePhaseBaseVm : ViewModel
{
	private Action m_NextStep;

	private Action m_BackStep;

	private readonly ReactiveProperty<bool> m_IsEnabled = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<bool> m_IsNextButtonAvailable = new ReactiveProperty<bool>();

	public ReadOnlyReactiveProperty<bool> IsEnabled => m_IsEnabled;

	public ReadOnlyReactiveProperty<bool> IsNextButtonAvailable => m_IsNextButtonAvailable;

	protected NewGamePhaseBaseVm(Action backStep, Action nextStep)
	{
		m_NextStep = nextStep;
		m_BackStep = backStep;
		m_IsEnabled.Value = false;
	}

	protected override void OnDispose()
	{
		m_NextStep = null;
		m_BackStep = null;
	}

	public virtual void OnNext()
	{
		ApplySettings();
		m_NextStep?.Invoke();
	}

	public virtual void OnBack()
	{
		m_BackStep?.Invoke();
	}

	public virtual bool SetEnabled(bool value, bool? direction = null)
	{
		m_IsEnabled.Value = value;
		return true;
	}

	public void SetNextButtonAvailable(bool available)
	{
		m_IsNextButtonAvailable.Value = available;
	}

	private void ApplySettings()
	{
		SettingsController.Instance.ConfirmAllTempValues();
		SettingsController.Instance.SaveAll();
	}
}
