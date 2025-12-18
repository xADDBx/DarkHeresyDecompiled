using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SettingsContext : ViewModel, ISettingsUIHandler, ISubscriber
{
	private readonly ReactiveProperty<SettingsVM> m_SettingsVM;

	public SettingsContext(ReactiveProperty<SettingsVM> settingsVM)
	{
		m_SettingsVM = settingsVM;
		EventBus.Subscribe(this).AddTo(this);
	}

	public void HandleOpenSettings(bool isMainMenu = false)
	{
		m_SettingsVM.Value = new SettingsVM(DisposeSettings, isMainMenu).AddTo(this);
	}

	private void DisposeSettings()
	{
		m_SettingsVM.CurrentValue?.Dispose();
		m_SettingsVM.Value = null;
	}
}
